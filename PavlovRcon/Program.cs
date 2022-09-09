namespace PavlovRcon
{
    class program
    {
        static void Main()
        {
            RConnection rcon = new RConnection("192.168.1.226", 9100); //Connect To Pavlov Server At 192.168.1.226 With Port 9100
            
            rcon.Login("21232f297a57a5a743894a0e4a801fc3"); //MD5SUM Of "admin"
            
            rcon.Command("GiveAll 1 de"); //Give All Red Team Members A Desert Eagle
            
            rcon.Read(); //Reads The Output From The Command Above
            
            Task.Delay(10); //Always Place A 10 MS Delay Between Commands To Improve Server Preformance
            
            foreach (string p in rcon.PlayerList()) //Loops For Every Player In The Server
            {
                Console.WriteLine(p); //Print Their Name
            }
            
            Task.Delay(10); //Always Place A 10 MS Delay Between Commands To Improve Server Preformance

            foreach (string i in rcon.ItemList()) //Loops For Every Item In The Game
            {
                Console.WriteLine(i); //Prints The Item Name
            }
        }        
    }
   
}