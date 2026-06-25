using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PROG_Part3
{
    // Data models and basic fields
    public class Cybertask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reminder { get; set; } //e.g . "2023-12-31" or "in 3 days"
        public bool Completed { get; set; }
    }
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public string Answer { get; set; }
        public string Explanation { get; set; }
        public bool IsTrueFalse { get; set; } // For true/false questions
    }
    public partial class Form1 : Form
    {
        //InitializeComponent();
        // MY SQL connection string
        private const string ConnStr =
        "Server=localhost;Database=cybertask_db;Uid=root;Pwd=;";

        //Memory and state
        private string username = "";
        private string favouriteTopic = "";
        private string lastTopic = "";
        private bool waitingForName = true;
        private bool isTyping = false;

        //Quiz state
        private bool inQuiz = false;
        private int quizIndex = 0;
        private int quizScore = 0;
        private bool awaitingQuizAnswer = false;
        private List<QuizQuestion> quizQuestions;
        public Form1()
        {

        }
    }
}