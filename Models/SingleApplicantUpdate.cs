namespace JobAppHR.Models
{
    public class SingleApplicantUpdate
    {
        public string ApplicationCode { get; set; } = "";
        public string IntakeCode { get; set; } = "";
        public string CurrentStage { get; set; } = "";

        /// <summary>
        /// The stage the page was loaded from (e.g., "0" for first-time, "FINAL" for subsequent edits)
        /// </summary>
        public string PreviousStage { get; set; } = "";

        public string NewStatus { get; set; } = "";
        public string OldStatus { get; set; } = "";
        public string NewRemarks { get; set; } = "";
    }
}
