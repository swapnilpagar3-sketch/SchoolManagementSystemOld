namespace EntityFrameworkCore.ViewModels
{
    public class ExamTopperVM
    {
        public string ExamName { get; set; }

        public string StudentName { get; set; }

        public int ObtainedMarks { get; set; }

        public int TotalMarks { get; set; }

        public double Percentage { get; set; }
    }
}
