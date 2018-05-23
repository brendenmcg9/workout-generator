using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace ConsoleApp1
{
    class WorkoutGen
    {
        //Input variables and lists
        private static int wLength = 0;
        private static String answer = "";
        private static String intensity = "";
        private static List<int> listNum;
        private static List<String> mGroups = new List<String>();

        static void Main(string[] args)
        {
            //MySql connection string and connection
            string connString = "Server=mcgrathj2sql.c9ygtrsflxxs.us-east-1.rds.amazonaws.com;Port=3306;Database=Test;Uid=root;Pwd=workout1";
            MySqlConnection conn = new MySqlConnection(connString);
            try
            {
                conn.Open();
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Init sql variables
            MySqlDataReader reader;
            MySqlCommand info = conn.CreateCommand();
            MySqlCommand minID = conn.CreateCommand();
            MySqlCommand maxID = conn.CreateCommand();   
            Random rand = new Random();

            //Begin prompt to select muscle group(s)
            //User enters in done to show that they are done selecting muscle groups
            while (!(answer.ToLower().Equals("done")))
            {
                Console.Write("Select muscle groups [Chest, Back, Legs, Biceps, Triceps, Abs]. Type 'done' when done selecting groups: ");
                answer = Console.ReadLine();
                //Checks to see if user entered in valid response
                if (answer.ToLower().Equals("chest") || answer.ToLower().Equals("back") || answer.ToLower().Equals("legs") || answer.ToLower().Equals("biceps") ||
                    answer.ToLower().Equals("triceps") || answer.ToLower().Equals("abs"))
                {
                    mGroups.Add(answer);
                }
                else if(answer.ToLower().Equals("done")) {
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Please enter in a valid muscle group.");
                }

            }
            //Prompts user to enter in a valid workout duration
            //User will continue to be prompted to enter in a value if not valid (short or normal)
            while (!(intensity.ToLower().Equals("short") || intensity.ToLower().Equals("normal")))
            {
                Console.Write("Select workout duration [Short, Normal]: ");
                intensity = Console.ReadLine();
                if (intensity.ToLower().Equals("short")) //Short duration is 3 exercises per muscle group (arbitrary number)
                    wLength = 3;
                else if (intensity.ToLower().Equals("normal")) //normal duration is 4 exercises per muscle group (arbitrary number)
                    wLength = 4;
                else
                    Console.WriteLine("Did not select proper workout length.");
            }
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Your workout consists of:");
            //Loops through list of muscle groups selected
            try
            {
                for (int i = 0; i < mGroups.Count; i++)
                {
                    minID.CommandText = "select min(ExerciseID) from Test.Exercise where muscle_group='" + mGroups[i] + "'"; //select statement for min(ExerciseID) of muscle group
                    maxID.CommandText = "select max(ExerciseID) from Test.Exercise where muscle_group='" + mGroups[i] + "'"; //select statement for max(ExerciseID) of muscle group
                    info.CommandText = "select * from Test.Exercise where muscle_group = '" + mGroups[i] + "'"; //select statement for all info from muscle group
                    reader = minID.ExecuteReader();
                    reader.Read();
                    int min = int.Parse(reader["min(ExerciseID)"].ToString()); //sets min id
                    reader.Close();
                    reader = maxID.ExecuteReader();
                    reader.Read();
                    int max = int.Parse(reader["max(ExerciseID)"].ToString()) + 1; //sets max id + 1 since rand uses an exclusive max
                    reader.Close();
                    reader = info.ExecuteReader();

                    //Generate random numbers to pick out exercises for each muscle group selected
                    int num = 0;
                    int temp = 0;
                    listNum = new List<int>();
                    for (int j = 0; j < wLength; j++) //wLength indicates how many exercises will be picked
                    {
                        num = rand.Next(min, max);
                        for (int k = 0; k < listNum.Count; k++) //Checks to see if there are any repeating numbers
                        {
                            if (num == listNum[k]) //Checks to see if there are any repeating numbers
                            {
                                temp = num;
                                while (num == temp) //If there is a repeating number found, generate a new number
                                    num = rand.Next(min, max);
                            }
                        }
                        listNum.Add(num);
                    }
                    //Reads data from selected command and writes the number of sets, reps, and name of the exercise to the console
                    while (reader.Read())
                    {
                        for (int m = 0; m < listNum.Count; m++)
                        {
                            if (reader["ExerciseID"].Equals(listNum[m])) //Checks what IDs are equal to the random IDs generated
                            {
                                Console.WriteLine(reader["set_num"].ToString() + " sets and " + reader["rep_num"].ToString() + " reps of " + reader["name"].ToString());
                            }
                        }
                    }
                    reader.Close();
                }
            } catch(SqlException ex) {
                Console.WriteLine(ex.Message);
            }
            Console.Read(); //Reads everything that was written to the console
            conn.Close();
        }
    }
}