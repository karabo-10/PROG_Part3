using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
        
        // MY SQL connection string
        private const string ConnStr =
        "Server=localhost;Database=cybertask_db;Uid=root;Pwd=;";

        //Memory and state
        private string userName = "";
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

        //Task assistant state
        private bool awaitingTaskTitle = false;
        private bool awaitingReminder = false;
        private string pendingTaskTitle = "";
        private string pendingTaskDesc = "";

        //Activity log
        private List<string> activityLog = new List<string>();

        //Keyword responses
        // ─── Keyword Responses ────────────────────────────────────────────────
        private Dictionary<string, List<string>> keywordResponses =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
            {
                "Use a strong password with a mix of letters, numbers, and symbols.",
                "Never reuse the same password across multiple sites. Consider a password manager like Bitwarden.",
                "Enable two-factor authentication (2FA) alongside a strong password for maximum security.",
                "Follow your organisation's policies when creating passwords."
            },
                ["phishing"] = new List<string>
            {
                "Phishing tricks you into revealing sensitive info through fake emails. Always check the sender's address!",
                "Be cautious of emails asking for personal information. Scammers disguise themselves as trusted organisations.",
                "Never click links in unexpected emails — type the website address yourself instead."
            },
                ["browsing"] = new List<string>
            {
                "Always use secure (https) websites and avoid unknown links.",
                "Consider uBlock Origin to block malicious ads and trackers.",
                "Keep your browser updated — updates include important security patches."
            },
                ["privacy"] = new List<string>
            {
                "Limit what you share online and review your app permissions regularly.",
                "Review your social media privacy settings to control who sees your information.",
                "Be mindful of personal data you share — once it's out there, it's hard to take back."
            },
                ["malware"] = new List<string>
            {
                "Malware is harmful software that damages your device or steals data. Avoid files from unknown sources!",
                "Keep your antivirus software up to date to defend against the latest threats.",
                "Regularly scan your device and be cautious of unexpected pop-ups."
            },
                ["scam"] = new List<string>
            {
                "Online scams try to trick you into giving away money or personal information.",
                "If an offer seems too good to be true, it probably is.",
                "Scammers create urgency — slow down and verify any unexpected requests."
            },
                ["wifi"] = new List<string>
            {
                "Avoid logging into sensitive accounts like banking on public Wi-Fi!",
                "Use a VPN on public Wi-Fi to protect your browsing data.",
                "Use your mobile data instead of public Wi-Fi for sensitive transactions."
            },
                ["virus"] = new List<string>
            {
                "Computer viruses are malicious programs. Use antivirus software and avoid unknown files!",
                "Keep your OS updated — many viruses exploit outdated software.",
                "Never open email attachments from unknown senders."
            }
            };

        //NLP synonym map(maps alternative words to a standard keyword)
        private Dictionary<string, string> nlpSynonyms = 
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["pass"] = "password",
                ["passwords"] = "password",
                ["credentials"] = "password",
                ["login"] = "password",
                ["phish"] = "phishing",
                ["spam"] = "phishing",
                ["fake email"] = "phishing",
                ["web"] = "browsing",
                ["internet"] = "browsing",
                ["surfing"] = "browsing",
                ["private"] = "privacy",
                ["data"] = "privacy",
                ["personal info"] = "privacy",
                ["virus"] = "virus",
                ["viruses"] = "virus",
                ["hack"] = "malware",
                ["hacker"] = "malware",
                ["ransomware"] = "malware",
                ["spyware"] = "malware",
                ["fraud"] = "scam",
                ["trick"] = "scam",
                ["network"] = "wifi",
                ["hotspot"] = "wifi",
                ["public wifi"] = "wifi"
            };
           
        //Sentiment map
        private Dictionary<string, string> sentimentPrefixes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["worried"]     = "It's understandable to feel that way. Here are some tips: ",
            ["scared"]      = "Don't worry — learning about this is already the right step. ",
            ["frustrated"]  = "I hear you — cybersecurity can feel overwhelming. Let's break it down: ",
            ["confused"]    = "No problem! Let me explain more clearly. ",
            ["curious"]     = "Great curiosity! Staying informed is the best defence. ",
            ["anxious"]     = "Take a deep breath — awareness is the first step to staying safe online. ",
            ["angry"]       = "Let's channel that energy into staying better protected. ",
            ["overwhelmed"] = "Let's take it one step at a time. Here's one thing you can do: "
        };

        //Follow up phrases
        private List<string> followUpPhrases = new List<string>
        {
            "tell me more", "another tip", "give me more", "explain more", "more info",
            "elaborate", "continue", "more please", "go on"
        };

        private Random rng = new Random();
    public Form1()
        {
            InitializeComponent();
            InitialiseQuizQuestions();
            EnsureDatabaseExists();
            this.Load += Form1_Load;
        }
        //Form load
        private void Form1_Load(object? sender, EventArgs e)
        {
            PlayVoiceGreeting();
            ShowAsciiLogo();
            AppendDivider();
            AppendSection("Welcome");
            AppendBot("Hello! I am your CyberSecurity Awareness Bot. 🛡");
            AppendBot("Ask me about password security, phishing, or safe browsing.");
            AppendBot("Type 'quiz' to start the mini-game, 'tasks' to manage tasks, or 'activity log' to view recent actions.");
            AppendDivider();
            AppendBot("Please enter your name to get started:");
        }
        //Send/enter buttons
        private void btnSend_Click(object sender, EventArgs e) => ProcessInput();

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ProcessInput(); }
        }
        private void ProcessInput()
        {
            if (isTyping) return;
            string input = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AppendUser(input);
            txtInput.Clear();

            //Name collection
            if (waitingForName)
            {
                if (!IsValidName(input))
                {

                    AppendError("Please enter a valid name using letters only (no numbers or symbols).");
                    return;
                }
                userName = input;
                waitingForName = false;
                AppendDivider();
                AppendSection("Welcome");
                TypeResponse($"Welcome {userName}! I am your CyberSecurity Awareness Bot.\n" +
                                 "Ask me about password security, phishing, or safe browsing.\n" +
                                 "Type 'quiz' to test your knowledge, 'tasks' to manage tasks, or 'activity log' to see recent actions.");
                return;
            }

            // Exit 
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
             AppendDivider();
             AppendColoured($"Goodbye {userName}! Stay safe online. 👋", Color.FromArgb(255, 220, 50));
             AppendDivider();
             btnSend.Enabled  = false;
             txtInput.Enabled = false;
             return;
            }

           // Multi-step: task title entry
            if (awaitingTaskTitle)
            {
                pendingTaskTitle = input;
                pendingTaskDesc  = $"Task: {input}";
                awaitingTaskTitle = false;
                awaitingReminder  = true;
                TypeResponse($"Got it! Task title: '{pendingTaskTitle}'.\nWould you like a reminder? If yes, type a date (e.g. 2025-07-01) or timeframe (e.g. 'in 3 days'). Otherwise type 'no'.");
                return;
            }
            
            // Reminder entry 
            if (awaitingReminder)
            {
                string reminder = input.Equals("no", StringComparison.OrdinalIgnoreCase) ? "" : input;
                awaitingReminder = false;
                SaveTaskToDb(pendingTaskTitle, pendingTaskDesc, reminder);
                string logEntry = $"Task added: '{pendingTaskTitle}'" +
                                  (string.IsNullOrEmpty(reminder) ? " (no reminder)." : $" | Reminder: {reminder}.");
                
                LogActivity(logEntry);
                string msg = $"Task '{pendingTaskTitle}' saved!";
                if (!string.IsNullOrEmpty(reminder)) msg += $" I'll remind you: {reminder}.";
                TypeResponse(msg);
                return;
            }
            
            // Quiz answer handling 
            if (inQuiz && awaitingQuizAnswer)
            {
                HandleQuizAnswer(input);
                return;
            }
 
            // General response 
            string response = BuildResponse(input);
            TypeResponse(response);
        }
           // Core Response Builder (NLP-enhanced) 
        private string BuildResponse(string raw)
{
    string lower = raw.ToLower();

    // sentiment prefix
    string sentimentPrefix = "";
    foreach (var kv in sentimentPrefixes)
        if (lower.Contains(kv.Key)) { sentimentPrefix = kv.Value; break; }

    // Activity log 
    if (lower.Contains("activity log") || lower.Contains("show log") ||
        lower.Contains("what have you done") || lower.Contains("recent actions"))
        return BuildActivityLogResponse();

    // Quiz trigger 
    if (lower.Contains("quiz") || lower.Contains("game") || lower.Contains("test me"))
    {
        StartQuiz();
        return null; // quiz start handles its own output
    }

    // Task triggers (NLP: many phrasings) 
    bool isAddTask = lower.Contains("add task") || lower.Contains("new task") ||
                     lower.Contains("create task") || lower.Contains("add a task") ||
                     (lower.Contains("add") && lower.Contains("task")) ||
                     lower.Contains("remind me to") || lower.Contains("set a reminder") ||
                     lower.Contains("set reminder");

    bool isViewTasks = lower.Contains("view task") || lower.Contains("show task") ||
                       lower.Contains("list task") || lower.Contains("my tasks") ||
                       lower.Contains("tasks");

    bool isDeleteTask = lower.Contains("delete task") || lower.Contains("remove task") ||
                        lower.Contains("complete task") || lower.Contains("done with task") ||
                        lower.Contains("mark complete");

    if (isAddTask)
    {
        // Try to extract task name inline, e.g. "add task enable 2FA"
        string inlineTitle = ExtractInlineTask(lower);
        if (!string.IsNullOrEmpty(inlineTitle))
        {
            pendingTaskTitle = inlineTitle;
            pendingTaskDesc = $"Task: {inlineTitle}";
            awaitingReminder = true;
            LogActivity($"Task creation started: '{inlineTitle}'.");
            return $"Task title detected: '{inlineTitle}'.\nWould you like a reminder? Type a date (e.g. 2025-07-01) or 'in 3 days', or type 'no'.";
        }
        else
        {
            awaitingTaskTitle = true;
            return $"Sure {userName}! What is the title of the task you'd like to add?";
        }
    }

    if (isDeleteTask)
    {
        return $"{userName}, to delete or complete a task, type: 'delete task [id]' or 'complete task [id]'.\nType 'my tasks' to see IDs.";
    }

    // Handle "delete task 3" or "complete task 2"
    if (lower.StartsWith("delete task ") || lower.StartsWith("remove task "))
    {
        string idStr = lower.Replace("delete task ", "").Replace("remove task ", "").Trim();
        if (int.TryParse(idStr, out int delId))
            return DeleteTaskFromDb(delId);
        return "Please provide a valid task ID number.";
    }

    if (lower.StartsWith("complete task "))
    {
        string idStr = lower.Replace("complete task ", "").Trim();
        if (int.TryParse(idStr, out int compId))
            return MarkTaskComplete(compId);
        return "Please provide a valid task ID number.";
    }

    if (isViewTasks)
        return GetTasksFromDb();

    // Memory recall 
    if (lower.Contains("what do you remember") || lower.Contains("what do you know about me"))
    {
        string mem = $"I remember your name is {userName}.";
        if (!string.IsNullOrEmpty(favouriteTopic))
            mem += $" You mentioned you're interested in {favouriteTopic}.";
        return mem;
    }

    if (lower.Contains("how are you"))
        return $"I'm functioning perfectly, {userName}! Ready to help you stay safe online.";

    if (lower.Contains("purpose"))
        return $"My purpose is to help you, {userName}, learn about cybersecurity and safe online practices.";

    if (lower.Contains("what can i ask") || lower.Contains("help") || lower.Contains("topics"))
        return $"{userName}, you can ask me about passwords, phishing, browsing, malware, scams, wifi, viruses, or type 'quiz' for a game!";

    if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
        return $"Hello {userName}! How can I help you stay safe online today?";

    // Follow-up
    foreach (var phrase in followUpPhrases)
    {
        if (lower.Contains(phrase) && !string.IsNullOrEmpty(lastTopic)
            && keywordResponses.ContainsKey(lastTopic))
            return sentimentPrefix + keywordResponses[lastTopic][rng.Next(keywordResponses[lastTopic].Count)];
    }

    // NLP: resolve synonyms first 
    string resolvedTopic = ResolveTopicNLP(lower);

    if (!string.IsNullOrEmpty(resolvedTopic) && keywordResponses.ContainsKey(resolvedTopic))
    {
        lastTopic = resolvedTopic;

        if (lower.Contains("interest") || lower.Contains("love") || lower.Contains("like") || lower.Contains("favourite"))
        {
            favouriteTopic = resolvedTopic;
            return $"Great! I'll remember you're interested in {resolvedTopic}. It's crucial for staying safe online.\n"
                 + keywordResponses[resolvedTopic][rng.Next(keywordResponses[resolvedTopic].Count)];
        }

        string personalised = (!string.IsNullOrEmpty(favouriteTopic) &&
                               favouriteTopic.Equals(resolvedTopic, StringComparison.OrdinalIgnoreCase))
            ? $"As someone interested in {favouriteTopic}, here's a tip: " : "";

        return sentimentPrefix + personalised + $"{userName}, " +
               keywordResponses[resolvedTopic][rng.Next(keywordResponses[resolvedTopic].Count)];
    }

    return $"Sorry {userName}, I didn't quite understand that. Could you rephrase? " +
           "Try asking about passwords, phishing, scams, privacy, malware, or type 'quiz' or 'tasks'.";
}

// ─── NLP: resolve topic from input (synonyms + direct keywords) ───────
private string ResolveTopicNLP(string lower)
{
    // Direct keyword match first
    foreach (var kv in keywordResponses)
        if (lower.Contains(kv.Key)) return kv.Key;

    // Synonym lookup
    foreach (var kv in nlpSynonyms)
        if (lower.Contains(kv.Key)) return kv.Value;

    return null;
}

// ─── Extract inline task title from input ─────────────────────────────
private string ExtractInlineTask(string lower)
{
    string[] prefixes = { "add task ", "new task ", "create task ", "add a task ", "remind me to ", "set a reminder to ", "set reminder to " };
    foreach (var p in prefixes)
    {
        if (lower.Contains(p))
        {
            int idx = lower.IndexOf(p) + p.Length;
            string title = lower.Substring(idx).Trim();
            if (!string.IsNullOrEmpty(title))
                return char.ToUpper(title[0]) + title.Substring(1);
        }
    }
    return null;
}
// Method for Quiz questions and answers functionality
private void InitialiseQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question    = "What should you do if you receive an email asking for your password?",
                    Options     = new[] { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                    Answer      = "C",
                    Explanation = "Reporting phishing emails helps protect others and trains spam filters.",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question    = "True or False: Using the same password for all accounts is safe.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "If one account is compromised, all accounts with the same password are at risk.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question    = "What does HTTPS indicate on a website?",
                    Options     = new[] { "A) The site is fast", "B) The connection is encrypted and secure", "C) The site is popular", "D) The site is free" },
                    Answer      = "B",
                    Explanation = "HTTPS uses SSL/TLS encryption to protect data between you and the website.",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question    = "True or False: Public Wi-Fi is always safe to use for banking.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "Public Wi-Fi can be intercepted. Use a VPN or mobile data for sensitive transactions.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question    = "What is two-factor authentication (2FA)?",
                    Options     = new[] { "A) Two passwords", "B) A second verification step beyond a password", "C) Two usernames", "D) A backup email" },
                    Answer      = "B",
                    Explanation = "2FA adds an extra layer of security, usually a code sent to your phone.",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question    = "Which of these is an example of social engineering?",
                    Options     = new[] { "A) Installing antivirus software", "B) Pretending to be IT support to get your password", "C) Updating your OS", "D) Using a VPN" },
                    Answer      = "B",
                    Explanation = "Social engineering manipulates people rather than systems to gain access.",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question    = "True or False: Antivirus software alone is enough to protect you from all cyber threats.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "You also need strong passwords, 2FA, updates, and good browsing habits.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question    = "What is ransomware?",
                    Options     = new[] { "A) Software that speeds up your PC", "B) Malware that encrypts your files and demands payment", "C) A type of antivirus", "D) A browser extension" },
                    Answer      = "B",
                    Explanation = "Ransomware locks your data and demands a ransom to restore access.",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question    = "True or False: You should click links in emails to verify they are real.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "Always go directly to the official website by typing the address yourself.",
                    IsTrueFalse = true
                },
                new QuizQuestion
                {
                    Question    = "Which password is the strongest?",
                    Options     = new[] { "A) password123", "B) MyName2000", "C) Tr0ub4dor&3!", "D) 12345678" },
                    Answer      = "C",
                    Explanation = "A strong password uses uppercase, lowercase, numbers, and special characters.",
                    IsTrueFalse = false
                },
                new QuizQuestion
                {
                    Question    = "What is a VPN used for?",
                    Options     = new[] { "A) Speeding up downloads", "B) Hiding your IP and encrypting internet traffic", "C) Blocking ads", "D) Storing passwords" },
                    Answer      = "B",
                    Explanation = "A VPN creates an encrypted tunnel for your internet connection.",
                    IsTrueFalse = false
                }
            };
        }
        private void StartQuiz()
        {
            inQuiz           = true;
            quizIndex        = 0;
            quizScore        = 0;
            awaitingQuizAnswer = false;
 
            // Shuffle questions
            for (int i = quizQuestions.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = quizQuestions[i];
                quizQuestions[i] = quizQuestions[j];
                quizQuestions[j] = tmp;
            }
 
            LogActivity("Quiz started.");
            AppendSection("Cybersecurity Quiz");
            AppendBot($"Let's test your cybersecurity knowledge, {userName}! There are {quizQuestions.Count} questions.\nType your answer (A/B/C/D or True/False) and press Enter.\n");
            ShowQuizQuestion();
        }
 
        private void ShowQuizQuestion()
        {
            if (quizIndex >= quizQuestions.Count)
            {
                EndQuiz();
                return;
            }
 
            var q = quizQuestions[quizIndex];
            string text = $"Q{quizIndex + 1}/{quizQuestions.Count}: {q.Question}\n";
 
            if (q.IsTrueFalse)
                text += "Answer True or False.";
            else
            {
                foreach (var opt in q.Options)
                    text += opt + "\n";
                text += "Enter A, B, C, or D.";
            }
 
            AppendBot(text);
            awaitingQuizAnswer = true;
        }
 
        private void HandleQuizAnswer(string raw)
        {
            awaitingQuizAnswer = false;
            var q      = quizQuestions[quizIndex];
            string ans = raw.Trim().ToUpper();
 
            // normalise True/False answers
            if (ans == "TRUE" || ans == "T")  ans = "True";
            if (ans == "FALSE" || ans == "F") ans = "False";
 
            bool correct = ans.Equals(q.Answer, StringComparison.OrdinalIgnoreCase);
 
            if (correct)
            {
                quizScore++;
                AppendColoured($"Correct! {q.Explanation}", Color.FromArgb(100, 220, 100));
            }
            else
            {
                AppendColoured($"Incorrect. The answer was: {q.Answer}\n{q.Explanation}", Color.FromArgb(255, 100, 100));
            }
 
            quizIndex++;
            Thread.Sleep(300);
            ShowQuizQuestion();
        }
 
        private void EndQuiz()
        {
            inQuiz = false;
            int pct = (int)((quizScore / (double)quizQuestions.Count) * 100);
 
            string grade;
            if      (pct >= 90) grade = "Outstanding! You're a cybersecurity pro!";
            else if (pct >= 70) grade = "Great job! You know your stuff.";
            else if (pct >= 50) grade = "Not bad, but keep learning to stay safe online!";
            else                grade = "Keep learning to stay safe online!";
 
            string result = $"Quiz complete! Your score: {quizScore}/{quizQuestions.Count} ({pct}%)\n{grade}";
            AppendColoured(result, Color.FromArgb(255, 220, 50));
            LogActivity($"Quiz completed — score {quizScore}/{quizQuestions.Count} ({pct}%).");
        }
//MySQL Database
  private void EnsureDatabaseExists()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    string sql = @"
                        CREATE TABLE IF NOT EXISTS tasks (
                            id          INT AUTO_INCREMENT PRIMARY KEY,
                            title       VARCHAR(255)  NOT NULL,
                            description TEXT,
                            reminder    VARCHAR(100),
                            completed   TINYINT(1) DEFAULT 0,
                            created_at  DATETIME DEFAULT CURRENT_TIMESTAMP
                        );";
                    new MySqlCommand(sql, conn).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                AppendError($"DB Error (setup): {ex.Message}");
            }
        }
 
        private void SaveTaskToDb(string title, string desc, string reminder)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    string sql = "INSERT INTO tasks (title, description, reminder) VALUES (@t,@d,@r)";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@t", title);
                    cmd.Parameters.AddWithValue("@d", desc);
                    cmd.Parameters.AddWithValue("@r", reminder);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { AppendError($"DB Error (save): {ex.Message}"); }
        }
 
        private string GetTasksFromDb()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    string sql = "SELECT id, title, reminder, completed FROM tasks ORDER BY id DESC LIMIT 20";
                    var reader = new MySqlCommand(sql, conn).ExecuteReader();
                    var lines  = new System.Text.StringBuilder();
                    lines.AppendLine($"📋 Your cybersecurity tasks, {userName}:");
                    bool any = false;
                    while (reader.Read())
                    {
                        any = true;
                        int    id        = reader.GetInt32(0);
                        string title     = reader.GetString(1);
                        string reminder  = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        bool   completed = reader.GetBoolean(3);
                        string status    = completed ? "✅ Done" : "🔲 Pending";
                        string rem       = string.IsNullOrEmpty(reminder) ? "" : $" | ⏰ {reminder}";
                        lines.AppendLine($"[{id}] {title}{rem} — {status}");
                    }
                    if (!any) lines.AppendLine("No tasks found. Type 'add task' to create one!");
                    return lines.ToString();
                }
            }
            catch (Exception ex) { return $"DB Error (read): {ex.Message}"; }
        }
 
        private string DeleteTaskFromDb(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM tasks WHERE id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0) return $"No task with ID {id} found.";
                    LogActivity($"Task ID {id} deleted.");
                    return $"Task ID {id} has been deleted.";
                }
            }
            catch (Exception ex) { return $"DB Error (delete): {ex.Message}"; }
        }
 
        private string MarkTaskComplete(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("UPDATE tasks SET completed=1 WHERE id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0) return $"No task with ID {id} found.";
                    LogActivity($"Task ID {id} marked as complete.");
                    return $"✅ Task ID {id} marked as complete! Well done, {userName}.";
                }
            }
            catch (Exception ex) { return $"DB Error (update): {ex.Message}"; }
        }

        //Activity log
        private void LogActivity(string description)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {description}";
            activityLog.Add(entry);
            // Keep only last 10
            if (activityLog.Count > 10)
                activityLog.RemoveAt(0);
        }
        private string BuildActivityLogResponse()
        {
            if (activityLog.Count == 0)
                return $"No actions logged yet, {userName}. Interact with the bot to generate activity!";
 
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Recent activity log for {userName}:");
            int num = 1;
            foreach (var entry in activityLog)
                sb.AppendLine($"{num++}. {entry}");
            return sb.ToString();
        }
 
        
        //UI HELPERS 
        
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            foreach (char c in name)
                if (!char.IsLetter(c)) return false;
            return true;
        }
 
        private void TypeResponse(string message)
        {
            if (message == null) return; // quiz handles its own output
 
            isTyping         = true;
            btnSend.Enabled  = false;
            txtInput.Enabled = false;
 
            // Log the response topic if detectable
            LogActivity($"Bot responded to user input.");
 
            var thread = new Thread(() =>
            {
                rtbChat.Invoke((Action)(() =>
                {
                    int s = rtbChat.TextLength;
                    rtbChat.AppendText("[CyberBot]: ");
                    rtbChat.Select(s, 12);
                    rtbChat.SelectionColor = Color.FromArgb(0, 200, 150);
                    rtbChat.SelectionFont  = new Font("Consolas", 10F, FontStyle.Bold);
                    rtbChat.SelectionLength = 0;
                }));
 
                foreach (char c in message)
                {
                    char ch = c;
                    rtbChat.Invoke((Action)(() =>
                    {
                        int pos = rtbChat.TextLength;
                        rtbChat.AppendText(ch.ToString());
                        rtbChat.Select(pos, 1);
                        rtbChat.SelectionColor = Color.FromArgb(200, 220, 255);
                        rtbChat.SelectionFont  = new Font("Consolas", 10F);
                        rtbChat.SelectionStart  = rtbChat.TextLength;
                        rtbChat.SelectionLength = 0;
                    }));
 
                    Thread.Sleep(c == '.' || c == '!' || c == '?' ? 150 : 20);
                }
 
                rtbChat.Invoke((Action)(() =>
                {
                    rtbChat.AppendText("\n\n");
                    rtbChat.ScrollToCaret();
                    isTyping         = false;
                    btnSend.Enabled  = true;
                    txtInput.Enabled = true;
                    txtInput.Focus();
                }));
            });
 
            thread.IsBackground = true;
            thread.Start();
        }
 
        private void ShowAsciiLogo()
        {
            string logo = @"
>>====================================================================<<
||              _                                        _ _          ||
||    ___ _   _| |__   ___ _ __ ___  ___  ___ _   _ _ __(_) |_ _   _  ||
||   / __| | | | '_ \ / _ \ '__/ __|/ _ \/ __| | | | '__| | __| | | | ||
||  | (__| |_| | |_) |  __/ |  \__ \  __/ (__| |_| | |  | | |_| |_| | ||
||   \___|\__, |_.__/ \___|_|  |___/\___|\___|\__,_|_|  |_|\__|\__, | ||
||        |___/                                       _        |___/  ||
||  __ ___      ____ _ _ __ ___ _ __   ___  ___ ___  | |__   ___ | |_ ||
|| / _` \ \ /\ / / _` | '__/ _ \ '_ \ / _ \/ __/ __| | '_ \ / _ \| __|||
||| (_| |\ V  V / (_| | | |  __/ | | |  __/\__ \__ \ | |_) | (_) | |_ ||
|| \__,_| \_/\_/ \__,_|_|  \___|_| |_|\___||___/___/ |_.__/ \___/ \__|||
>>====================================================================<<
                        /-----\
                       | 0   0 |
                       |   ^   |
                       |  ---  |
                       |_______|
                        /| | |\
                       /_|_|_|_\
                    Stay Cyber Safe
";
            int start = rtbChat.TextLength;
            rtbChat.AppendText(logo + "\n");
            rtbChat.Select(start, logo.Length);
            rtbChat.SelectionColor = Color.FromArgb(0, 210, 210);
            rtbChat.SelectionFont  = new Font("Consolas", 9F, FontStyle.Bold);
            rtbChat.ScrollToCaret();
        }
 
        private void AppendSection(string title)
        {
            int start = rtbChat.TextLength;
            string text = $"\n=== {title} ===\n";
            rtbChat.AppendText(text);
            rtbChat.Select(start, text.Length);
            rtbChat.SelectionColor = Color.FromArgb(220, 100, 255);
            rtbChat.SelectionFont  = new Font("Consolas", 11F, FontStyle.Bold);
        }
 
        private void AppendDivider()
        {
            int start = rtbChat.TextLength;
            string line = "\n" + new string('=', 70) + "\n";
            rtbChat.AppendText(line);
            rtbChat.Select(start, line.Length);
            rtbChat.SelectionColor = Color.FromArgb(80, 80, 80);
        }
 
        private void AppendBot(string message)
        {
            int s = rtbChat.TextLength;
            rtbChat.AppendText("[CyberBot]: ");
            rtbChat.Select(s, 12);
            rtbChat.SelectionColor = Color.FromArgb(0, 200, 150);
            rtbChat.SelectionFont  = new Font("Consolas", 10F, FontStyle.Bold);
 
            int ms = rtbChat.TextLength;
            rtbChat.AppendText(message + "\n\n");
            rtbChat.Select(ms, message.Length);
            rtbChat.SelectionColor = Color.FromArgb(200, 220, 255);
            rtbChat.SelectionFont  = new Font("Consolas", 10F);
            rtbChat.ScrollToCaret();
        }
 
        private void AppendUser(string message)
        {
            string label = string.IsNullOrEmpty(userName) ? "[You]: " : $"[{userName}]: ";
            int s = rtbChat.TextLength;
            rtbChat.AppendText(label);
            rtbChat.Select(s, label.Length);
            rtbChat.SelectionColor = Color.FromArgb(100, 200, 100);
            rtbChat.SelectionFont  = new Font("Consolas", 10F, FontStyle.Bold);
 
            int ms = rtbChat.TextLength;
            rtbChat.AppendText(message + "\n");
            rtbChat.Select(ms, message.Length);
            rtbChat.SelectionColor = Color.White;
            rtbChat.SelectionFont  = new Font("Consolas", 10F);
            rtbChat.ScrollToCaret();
        }
 
        private void AppendError(string message)
        {
            int s = rtbChat.TextLength;
            rtbChat.AppendText(message + "\n\n");
            rtbChat.Select(s, message.Length);
            rtbChat.SelectionColor = Color.FromArgb(255, 80, 80);
            rtbChat.SelectionFont  = new Font("Consolas", 10F);
        }
 
        private void AppendColoured(string message, Color color)
        {
            int s = rtbChat.TextLength;
            rtbChat.AppendText(message + "\n\n");
            rtbChat.Select(s, message.Length);
            rtbChat.SelectionColor = color;
            rtbChat.SelectionFont  = new Font("Consolas", 10F, FontStyle.Bold);
        }
 
        private void PlayVoiceGreeting()
        {
            if (OperatingSystem.IsWindows())
            {
                try { new SoundPlayer("greeting.wav").Play(); } catch { }
            }
        }
 
        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbChat.Clear();
            ShowAsciiLogo();
            AppendBot($"Chat cleared! How can I help you, {userName}?");
        }
    }
}


