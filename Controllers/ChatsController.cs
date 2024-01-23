using Learning_Space.DTO;
using Learning_Space.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFile = System.IO.File;

namespace Learning_Space.Controllers
{
    public class ChatsController : Controller
    {
        private readonly LearningSpaceContext _context;

        public ChatsController(LearningSpaceContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index(int user,int courseid)
        {
            List<ChatMessageDTO> messages =  new List<ChatMessageDTO>();
            
            string filePath = Path.Combine(".", "TextFiles", "Chats", "Courses", $"{courseid}" + ".txt");

            // Reads all lines from the specified file path
            string[] lines = MyFile.ReadAllLines(filePath);

            // Retrieves the user information from the database
            var useri = await _context.Users.FirstOrDefaultAsync(m => m.UserId == user);

            foreach (string line in lines)
            {
                // Splits each line into parts based on the comma separator
                string[] parts = line.Split(',');

                ChatMessageDTO message = new ChatMessageDTO
                {
                    SenderFirstName = parts[0],
                    SenderLastName = parts[1],
                    // Parses the sent date and time
                    SentDateTime = DateTime.Parse(parts[2]),
                    Body = parts[3],
                    // Checks if the current user is the sender
                    ISender = (parts[0] == useri.FirstName && parts[1] == useri.LastName) ? 1 : 0
                };

                // Adds the message to the list of messages
                messages.Add(message);
            }

            return View(messages.AsEnumerable());
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMessage(int user, int permission, int courseid, ChatMessageDTO message)
        {
            // Implement the logic to delete the message from the chat text file based on the provided parameters
            string filePath = Path.Combine(".", "TextFiles", "Chats", "Courses", $"{courseid}" + ".txt");

            // Read all lines from the file
            string[] lines = await MyFile.ReadAllLinesAsync(filePath);

            // Find the line that matches the provided message details
            string messageString = $"{message.SenderFirstName},{message.SenderLastName},{message.SentDateTime},{message.Body}";
            string lineToRemove = lines.FirstOrDefault(line => line == messageString);

            // Remove the line from the lines array
            if (lineToRemove != null)
            {
                lines = lines.Where(line => line != lineToRemove).ToArray();
                // Write the updated lines back to the file
                await MyFile.WriteAllLinesAsync(filePath, lines);
        }

            return Redirect($"/Chats/Index?user={user}&permission={permission}&courseid={courseid}");

        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(int user, int permission, int courseid, ChatMessageDTO message)
        {
            // Set the date and time the message was sent
            message.SentDateTime = DateTime.Now ;

            string filePath = Path.Combine(".", "TextFiles", "Chats", "Courses", $"{courseid}" + ".txt");
            var useri = await _context.Users.FirstOrDefaultAsync(m => m.UserId == user);

            // Save the message
            string messageString = $"{useri.FirstName},{useri.LastName},{message.SentDateTime},{message.Body}";

            MyFile.AppendAllText(filePath, messageString + Environment.NewLine);

            return Redirect($"/Chats/Index?user={user}&permission={permission}&courseid={courseid}");
        }
   }
}
