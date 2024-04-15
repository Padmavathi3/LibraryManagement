using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }

        // Constructor to initialize book properties.
        public Book(string title, string author, string isbn)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
        }
    }
    //--------------------------------------------------------------------------------------
    public class User
    {
        public string Name { get; set; }
        public int ID { get; }

        // Constructor to initialize user properties.
        public User(string name, int id)
        {
            Name = name;
            ID = id;
        }
    }
    //---------------------------------------------------------------------------------------------------
    public class Library
    {
        private List<Book> books= new List<Book>(); // List of books
        private Dictionary<User, List<Book>> borrowedBooks= new Dictionary<User, List<Book>>(); // list of users with coreespondim books borrowed by them

        // Method to add a book to the library.
        public void AddBook(Book book)
        {
            books.Add(book);
        }

        // Method to borrow a book by a user.
        public void BorrowBook(User user, Book book)
        {
            lock (books) // Lock access to the books collection to ensure thread safety
            {
                if (!books.Contains(book))
                {
                    Console.WriteLine("Book is not available in the library.");
                    return;
                }

                lock (borrowedBooks) // Lock access to borrowedBooks dictionary
                {
                    if (!borrowedBooks.ContainsKey(user))  // if user is not existed in the library dictionary it will create new enrtry for that user
                    {
                        borrowedBooks[user] = new List<Book>();
                    }

                    borrowedBooks[user].Add(book);       // if use r already existing then one more book is added into that user list of books in dictionary 
                    books.Remove(book);
                    Console.WriteLine($"{user.Name} borrowed {book.Title}.");
                }
            }
        }

        // Method to return a book by a user.
        public void ReturnBook(User user, Book book)
        {
            lock (borrowedBooks) // Lock access to borrowedBooks dictionary
            {
                if (!borrowedBooks.ContainsKey(user) || !borrowedBooks[user].Contains(book)) //it will check whether the user  alredy borrowed this book or not
                {
                    Console.WriteLine("User has not borrowed this book.");
                    return;
                }

                lock (books) // Lock access to the books collection        //if user returns the book we have to remove the book from that dictionary and agin we have to add this book inside library books list
                {
                    borrowedBooks[user].Remove(book);
                    books.Add(book);
                    Console.WriteLine($"{user.Name} returned {book.Title}.");
                }
            }
        }

        // Method to get available books in the library.
        public IEnumerable<Book> GetAvailableBooks()
        {
            lock (books) // Lock access to the books collection
            {
                return books.ToList(); // Return a copy to avoid modification outside of the lock
            }
        }

        // Method to get books borrowed by a specific user.
        public IEnumerable<Book> GetBorrowedBooksByUser(User user)
        {
            lock (borrowedBooks) // Lock access to borrowedBooks dictionary
            {
                if (borrowedBooks.ContainsKey(user))
                {
                    return borrowedBooks[user].ToList(); // Return a copy to avoid modification outside of the lock
                }
                return Enumerable.Empty<Book>(); // Return an empty collection if user hasn't borrowed any books
            }
        }
    }
    class Mangement
    {
        static void Main(string[] args)
        {
            //object for library class
            Library library = new Library();

            // Creating some sample books
            Book book1 = new Book("python Programming", "F. Scott Fitzgerald", "9780743273565");
            Book book2 = new Book("c programming", "Harper Lee", "9780061120084");
            Book book3 = new Book("Java", "Yuval Noah Harari", "9780062316097");
            

            // Adding books to the library
            library.AddBook(book1);
            library.AddBook(book2);
            library.AddBook(book3);

            // Creating sample users
            User user1 = new User("padma", 100);
            User user2 = new User("Latha", 101);
            User user3 = new User("Raghu", 103);

            // Create threads for borrowing books
            Console.WriteLine($"Checking whether {user1.Name} borrows {book1.Title} or not");
            Thread borrowThread1 = new Thread(() => library.BorrowBook(user1, book1)); // Creates a new Thread with a lambda function without parameters
            Console.WriteLine($"Checking whether {user2.Name} borrows {book2.Title} or not");
            Thread borrowThread2 = new Thread(() => library.BorrowBook(user2, book2));

            // Create threads for returning books
            Console.WriteLine($"Checking whether {user1.Name} returns {book1.Title} or not");
            Thread returnThread1 = new Thread(() => library.ReturnBook(user1, book1));
            Console.WriteLine($"Checking whether {user2.Name} returns {book2.Title} or not");
            Thread returnThread2 = new Thread(() => library.ReturnBook(user2, book2));


            // Starting threads
            borrowThread1.Start();
            borrowThread2.Start();
            returnThread1.Start();
            returnThread2.Start();

            // Waiting for threads to finish
            borrowThread1.Join();
            borrowThread2.Join();
            returnThread1.Join();
            returnThread2.Join();


            // Using LINQ to query available books from the library
            var availableBooks = library.GetAvailableBooks()
                                        .OrderBy(book => book.Title); // Example sorting by title

            // Printing available books
            Console.WriteLine("Available Books:");
            Console.WriteLine();
            foreach (var book in availableBooks)
            {
                Console.WriteLine($"{book.Title} by {book.Author}");
            }


            // Using LINQ to get borrowed books by a user
            var borrowedBooksByUser = library.GetBorrowedBooksByUser(user2);
            Console.WriteLine($"{user2.Name}'s Borrowed Books:");
            Console.WriteLine();
            foreach (var book in borrowedBooksByUser)
            {
                Console.WriteLine($"{book.Title} by {book.Author}");
            }
        }
    }
}
