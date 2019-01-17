using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace restapi.Models
{
    public class Timecard
    {
        public Timecard(int resource)
        {
            Resource = resource;
            UniqueIdentifier = Guid.NewGuid();
            Identity = new TimecardIdentity();
            Lines = new List<AnnotatedTimecardLine>();
            Transitions = new List<Transition> { 
                new Transition(new Entered() { Resource = resource }) 
            };
        }

        public int Resource { get; private set; }
        
        [JsonProperty("id")]
        public TimecardIdentity Identity { get; private set; }

        public TimecardStatus Status { 
            get 
            { 
                return Transitions
                    .OrderByDescending(t => t.OccurredAt)
                    .First()
                    .TransitionedTo;
            } 
        }

        public DateTime Opened;

        [JsonProperty("recId")]
        public int RecordIdentity { get; set; } = 0;

        [JsonProperty("recVersion")]
        public int RecordVersion { get; set; } = 0;

        public Guid UniqueIdentifier { get; set; }

        [JsonIgnore]
        public IList<AnnotatedTimecardLine> Lines { get; set; }

        [JsonIgnore]
        public IList<Transition> Transitions { get; set; }

        public IList<ActionLink> Actions { get => GetActionLinks(); }
    
        [JsonProperty("documentation")]
        public IList<DocumentLink> Documents { get => GetDocumentLinks(); }

        public string Version { get; set; } = "timecard-0.1";

        private IList<ActionLink> GetActionLinks()
        {
            var links = new List<ActionLink>();

            switch (Status)
            {
                case TimecardStatus.Draft:
                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Cancellation,
                        Relationship = ActionRelationship.Cancel,
                        Reference = $"/timesheets/{Identity.Value}/cancellation"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Submittal,
                        Relationship = ActionRelationship.Submit,
                        Reference = $"/timesheets/{Identity.Value}/submittal"
                    });

                    links.Add(new ActionLink() {
                        
                        Method = Method.Post,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.RecordLine,
                        Reference = $"/timesheets/{Identity.Value}/lines"
                    });

                    links.Add(new ActionLink(){
                        Method = Method.Delete,
                        Relationship = ActionRelationship.Remove,
                        Reference = $"/timesheets/{Identity.Value}/removal"
                    });
                    if(Lines.Count > 0){
                        links.Add(new ActionLink(){
                        Method = Method.Post,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.ReplaceLine,
                        Reference = $"/timesheets/{Identity.Value}/replaceLine/<LineNumber>"
                        });

                        links.Add(new ActionLink(){
                            Method = Method.Patch,
                            Type = ContentTypes.TimesheetLine,
                            Relationship = ActionRelationship.UpdateLineItem,
                            Reference = $"/timesheets/{Identity.Value}/updateItem/<LineNumber>"
                        });
                    }
                    
                
                    break;

                case TimecardStatus.Submitted:
                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Cancellation,
                        Relationship = ActionRelationship.Cancel,
                        Reference = $"/timesheets/{Identity.Value}/cancellation"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Rejection,
                        Relationship = ActionRelationship.Reject,
                        Reference = $"/timesheets/{Identity.Value}/rejection"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Approval,
                        Relationship = ActionRelationship.Approve,
                        Reference = $"/timesheets/{Identity.Value}/approval"
                    });

                    break;

                case TimecardStatus.Approved:
                    // terminal state, nothing possible here
                    break;

                case TimecardStatus.Cancelled:
                    // exposing the delete option once the timecard is cancelled.
                    links.Add(new ActionLink(){
                        Method = Method.Delete,
                        Relationship = ActionRelationship.Remove,
                        Reference = $"/timesheets/{Identity.Value}/removal"
                    });
                    break;
            }

            return links;
        }

        private IList<DocumentLink> GetDocumentLinks()
        {
            var links = new List<DocumentLink>();

            links.Add(new DocumentLink() {
                Method = Method.Get,
                Type = ContentTypes.Transitions,
                Relationship = DocumentRelationship.Transitions,
                Reference = $"/timesheets/{Identity.Value}/transitions"
            });

            if (this.Lines.Count > 0)
            {
                links.Add(new DocumentLink() {
                    Method = Method.Get,
                    Type = ContentTypes.TimesheetLine,
                    Relationship = DocumentRelationship.Lines,
                    Reference = $"/timesheets/{Identity.Value}/lines"
                });
            }

            if (this.Status == TimecardStatus.Submitted)
            {
                links.Add(new DocumentLink() {
                    Method = Method.Get,
                    Type = ContentTypes.Transitions,
                    Relationship = DocumentRelationship.Submittal,
                    Reference = $"/timesheets/{Identity.Value}/submittal"
                });
            }

            return links;
        }

        public AnnotatedTimecardLine AddLine(TimecardLine timecardLine,int lineNum)
        {
            var annotatedLine = new AnnotatedTimecardLine(timecardLine);
            
            annotatedLine.LineNumber = lineNum;
            Lines.Add(annotatedLine);
            

            return annotatedLine;
        }

        /**
            Method that takes the timecard line and line number as input and replaces the line 
            corresponding to the line number in the database with the new contents.
        */
        public AnnotatedTimecardLine replaceLine(TimecardLine timecardLine,int lineNum)
        {
            var oldtimecardLine = new AnnotatedTimecardLine(timecardLine);
            oldtimecardLine.LineNumber = lineNum;
            Lines.Remove(oldtimecardLine);
            return AddLine(timecardLine,lineNum); // adding the same line that was supposed to be replaced.
        }

        /**
            Method that updates the items in the line which have been modified by the user.
            This is done based on the line number.
         */
        public AnnotatedTimecardLine updateLineItem(TimecardLine timecardLine, int lineNum)
        {
            // The Lines are stored in the list at positions coressponding to their lineNum-1
            
            var storedTimeCardLine = Lines.ElementAtOrDefault(lineNum-1);
            if(timecardLine.Week != 0){
                storedTimeCardLine.Week = timecardLine.Week;
            }
            if(timecardLine.Day != storedTimeCardLine.Day){
                storedTimeCardLine.Day = timecardLine.Day;
            }
            if(timecardLine.Year != 0){
                storedTimeCardLine.Year = timecardLine.Year;
            }
            if(timecardLine.Hours != 0){
                storedTimeCardLine.Hours = timecardLine.Hours;
            }

            if(timecardLine.Project != null){
                storedTimeCardLine.Project = timecardLine.Project;
            }
            return storedTimeCardLine;
        }
    }
} 