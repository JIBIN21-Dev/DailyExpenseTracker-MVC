using project1.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace project1.Controllers
{
    public class HomeController : Controller
    {
        public string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Users user)
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExpenseTrackerDB;Integrated Security=True;Encrypt=False";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Check if email already exists
                string checkQuery = "SELECT COUNT(*) FROM jan WHERE Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@Email", user.Email);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        TempData["AlertMessage"] = "Email already registered";
                        TempData["AlertType"] = "error";
                        return View();
                    }
                }

                // Insert new user
                string insertQuery = "INSERT INTO jan (Name, Email, Password) VALUES (@Name, @Email, @Password)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    string hashedPassword = HashPassword(user.Password);

                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        TempData["AlertMessage"] = "Registration Successful!";
                        TempData["AlertType"] = "success";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["AlertMessage"] = "Registration Failed";
                        TempData["AlertType"] = "error";
                    }
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Login(Users user)
        {
            string cs = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExpenseTrackerDB;Integrated Security=True;Encrypt=False";

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                string hashedPassword = HashPassword(user.Password);

                string sql = "SELECT Id, Name, Email FROM jan WHERE Email=@Email AND Password=@Password";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        Session["UserId"] = dr["Id"];
                        Session["UserName"] = dr["Name"];
                        Session["UserEmail"] = dr["Email"];
                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                       
                        TempData["AlertMessage"] = "Invalid email or password";
                        TempData["AlertType"] = "error";
                        return RedirectToAction("Index");
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult Dashboard(Users user)
        {
            return Login(user);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        // Dashboard View
        public ActionResult Dashboard()
        {
            if (Session["UserId"] == null)
               return RedirectToAction("Index");

            int userId = (int)Session["UserId"];

            decimal totalIncome = 0;
            decimal totalExpense = 0;
            List<Income> incomeList = new List<Income>();
            List<Expense> expenseList = new List<Expense>();

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExpenseTrackerDB;Integrated Security=True;Encrypt=False";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Get Income
                SqlCommand cmdIncome = new SqlCommand("SELECT * FROM Income WHERE UserId=@UserId ORDER BY Date DESC", con);
                cmdIncome.Parameters.AddWithValue("@UserId", userId);
                SqlDataReader reader1 = cmdIncome.ExecuteReader();
                while (reader1.Read())
                {
                    incomeList.Add(new Income
                    {
                        Id = (int)reader1["Id"],
                        Amount = (decimal)reader1["Amount"],
                        Category = reader1["Category"].ToString(),
                        Date = Convert.ToDateTime(reader1["Date"])
                    });
                    totalIncome += (decimal)reader1["Amount"];
                }
                reader1.Close();

                // Get Expenses
                SqlCommand cmdExp = new SqlCommand("SELECT * FROM Expenses WHERE UserId=@UserId ORDER BY Date DESC", con);
                cmdExp.Parameters.AddWithValue("@UserId", userId);
                SqlDataReader reader2 = cmdExp.ExecuteReader();
                while (reader2.Read())
                {
                    expenseList.Add(new Expense
                    {
                        Id = (int)reader2["Id"],
                        Amount = (decimal)reader2["Amount"],
                        Category = reader2["Category"].ToString(),
                        Date = Convert.ToDateTime(reader2["Date"])
                    });
                    totalExpense += (decimal)reader2["Amount"];
                }
                reader2.Close();
            }

            // Fixed: Use consistent casing
            ViewBag.totalIncome = totalIncome;
            ViewBag.totalExpense = totalExpense;
            ViewBag.Balance = totalIncome - totalExpense;
            ViewBag.incomeList = incomeList;
            ViewBag.expenseList = expenseList;

            // Fixed: Use correct session key
            ViewBag.Username = Session["UserName"];
            ViewBag.UserEmail = Session["UserEmail"];

            return View();
        }

        [HttpPost]
        public ActionResult AddIncome(decimal amount, string category, DateTime date)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Index");

            int userId = (int)Session["UserId"];
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExpenseTrackerDB;Integrated Security=True;Encrypt=False";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Income(UserId, Amount, Category, Date) VALUES (@UserId, @Amount, @Category, @Date)", con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public ActionResult AddExpense(decimal amount, string category, DateTime date)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Index");

            int userId = (int)Session["UserId"];
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExpenseTrackerDB;Integrated Security=True;Encrypt=False";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Expenses(UserId, Amount, Category, Date) VALUES (@UserId, @Amount, @Category, @Date)", con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Dashboard");
        }
    }
}