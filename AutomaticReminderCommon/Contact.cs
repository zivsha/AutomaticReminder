using System;

namespace AutomaticReminderCommon
{
    public class Contact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Contact(string first, string last, string email, string phoneNumber)
        {
            FirstName = first;
            LastName = last;
            Email = email;
            PhoneNumber = phoneNumber;
        }
        public Contact()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            Email = String.Empty;
            PhoneNumber = String.Empty;
        }
    }
}