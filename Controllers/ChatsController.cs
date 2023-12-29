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

            string[] lines = MyFile.ReadAllLines(filePath);

            var useri = await _context.Users.FirstOrDefaultAsync(m => m.UserId == user);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                ChatMessageDTO message = new ChatMessageDTO
                {
                   

                    SenderFirstName = parts[0],
                    SenderLastName = parts[1],
                    SentDateTime = DateTime.Parse(parts[2]),
                    Body = parts[3],
                    ISender = (parts[0] == useri.FirstName && parts[1] == useri.LastName) ? 1 : 0
                };

                messages.Add(message);
            }

            return View(messages.AsEnumerable());
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(int user, int permission, int courseid, ChatMessageDTO message)
        {
            // Set the date and time the message was sent
            message.SentDateTime = DateTime.Now;

            string filePath = Path.Combine(".", "TextFiles", "Chats", "Courses", $"{courseid}" + ".txt");
            var useri = await _context.Users.FirstOrDefaultAsync(m => m.UserId == user);

            // Save the message
            string messageString = $"{useri.FirstName},{useri.LastName},{message.SentDateTime},{message.Body}";

            MyFile.AppendAllText(filePath, messageString + Environment.NewLine);

            return Redirect($"/Chats/Index?user={user}&permission={permission}&courseid={courseid}");
        }
   }
}
