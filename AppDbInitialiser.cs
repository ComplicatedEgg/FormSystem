using APIClasses;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text;

namespace DataWebService.Contexts
{
    public static class AppDbInitialiser
    {
        public static async Task Initialise(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Clear existing data
            context.Accounts.RemoveRange(context.Accounts);
            context.Transactions.RemoveRange(context.Transactions);
            context.Profiles.RemoveRange(context.Profiles);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Generate new random data
            var random = new Random();
            var profiles = new List<Profile>();
            var accounts = new List<Account>();
            var transactions = new List<Transaction>();

            string[] firstNames = { "Peter", "Albert", "Sean", "Adam", "Izaac", "Arthur", "Mason", "Kiara", "Julianna", "Kanika", "Toni", "Izhary", "Hana", "Leanne" };
            string[] lastNames = { "Stoyanov", "Ong", "Kwan", "Whittome", "Murray", "Bradley", "Calter", "Bailey", "Fortun", "Abbott", "Kim", "Suaverdez", "Lee", "Doe" };

            // Generate 10 basic icons
            List<Bitmap> icons = [];
            for (int i = 0; i < 10; i++)
            {
                var image = new Bitmap(16, 16);
                for (var x = 0; x < 16; x++)
                {
                    for (var y = 0; y < 16; y++)
                    {
                        image.SetPixel(x, y, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                    }
                }
                icons.Add(image);
            }

            // Generate 10 Profiles
            for (int i = 0; i < 10; i++)
            {
                profiles.Add(new Profile
                {
                    ProfileId = i,
                    Username = GenerateUsername(),
                    Password = GeneratePassword(12),
                    FirstName = firstNames[random.Next(firstNames.Length)],
                    LastName = lastNames[random.Next(firstNames.Length)],
                    Email = GenerateEmail(),
                    Address = GenerateAddress(),
                    Telephone = GeneratePhoneNumber(),
                    Picture = BitmapToByteArray(icons[i]),
                });
            }

            // Generate an Account for each Profile
            for (int i = 0; i < 10; i++)
            {
                accounts.Add(new Account
                {
                    AccountId = i,
                    HolderFirstName = profiles[i].FirstName,
                    HolderLastName = profiles[i].LastName,
                    Balance = random.Next(100, 10000)
                });
            }

            // Simulate 100 random Transactions (Withdrawals, Deposits, Transfers)
            for (int i = 0; i < 100; i++)
            {
                int sender = random.Next(accounts.Count);
                int receiver;
                do // Make sure sender and receiver id are not identical
                {
                    receiver = random.Next(accounts.Count);
                } while (sender == receiver);

                var multiplier = random.NextDouble() * 0.8 + 0.1; // Random multiplier between 0.1 and 0.9
                double amount = 0;
                switch (random.Next(3))
                {
                    case 0: // Withdrawal
                        amount = accounts[sender].Balance * multiplier;
                        accounts[sender].Balance -= Math.Round(amount, 2);
                        break;
                    case 1: // Deposit
                        amount = accounts[receiver].Balance * (multiplier + 1);
                        accounts[receiver].Balance += Math.Round(amount, 2);
                        break;
                    case 2: // Transfer
                        amount = accounts[sender].Balance * multiplier;
                        accounts[sender].Balance -= Math.Round(amount, 2);
                        accounts[receiver].Balance += Math.Round(amount, 2);
                        break;
                }

                transactions.Add(new Transaction
                {
                    TransactionId = i,
                    SenderId = sender,
                    ReceiverId = receiver,
                    Amount = amount
                });
            }

            await context.Accounts.AddRangeAsync(accounts);
            await context.Transactions.AddRangeAsync(transactions);
            await context.Profiles.AddRangeAsync(profiles);
            await context.SaveChangesAsync();
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        static string GenerateUsername()
        {
            const string numbers = "0123456789";
            var username = new StringBuilder();
            var random = new Random();

            string[] firstHalf = { "Brave", "Calm", "Mighty", "Handsome", "Fancy", "Gentle", "Happy", "Icy", "Jolly", "Lively", "Noble", "Rapid", "Silly", "Tiny", "Unique", "Warm" };
            string[] secondHalf = { "Apple", "Bridge", "Rocket", "Dragon", "Engine", "Forest", "Star", "Helmet", "Island", "Kitten", "Puppy", "Market", "Ladder", "Orange", "Planet", "Lamp" };
            username.Append(firstHalf[random.Next(firstHalf.Length)]).Append(secondHalf[random.Next(secondHalf.Length)]).Append(numbers[random.Next(numbers.Length)]).Append(numbers[random.Next(numbers.Length)]);

            return username.ToString();
        }

        static string GeneratePassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var password = new StringBuilder();
            var random = new Random();

            while (0 < length--)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            return password.ToString();
        }

        static string GenerateEmail()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var localPart = new StringBuilder();
            var random = new Random();

            // Generate the part of the email before the @
            for (int i = 0; i < 8; i++)
            {
                localPart.Append(validChars[random.Next(validChars.Length)]);
            }

            string[] domains = { "gmail.com", "outlook.com", "icloud.com", "yahoo.com" };
            string domain = domains[random.Next(domains.Length)];

            return $"{localPart}@{domain}";
        }

        static string GenerateAddress()
        {
            var address = new StringBuilder();
            var random = new Random();

            //Generate unit or apartment?
            int number = random.Next(1, 30);
            switch (random.Next(3))
            {
                case 0: // Do not generate
                    break;
                case 1: // Generate a unit
                    address.Append("Unit ").Append(number).Append(", ");
                    break;
                case 2: // Generate an apartment
                    address.Append("Apartment ").Append(number).Append(", ");
                    break;
            }

            // Generate street number
            int streetNumber = random.Next(1, 100);
            address.Append(streetNumber).Append(" ");

            // Generate street name
            string[] streetNames = { "Main", "High", "Maple", "Oak", "Pine", "Cedar", "Elm", "Birch", "Lakeview", "Hillside", "Almondbury", "Walnut", "Curtin", "Garden", "North", "East", "South", "West" };
            string streetName = streetNames[random.Next(streetNames.Length)];
            address.Append(streetName).Append(" ");

            // Generate street type
            string[] streetTypes = { "Street", "Avenue", "Boulevard", "Lane", "Road", "Drive", "Terrace", "Place", "Loop", "Crescent", "Way", "Loop", "Parade", "Pass", "Highway", "Parkway", "Circle", "Square" };
            string streetType = streetTypes[random.Next(streetTypes.Length)];
            address.Append(streetType);

            return address.ToString();
        }

        static string GeneratePhoneNumber()
        {
            const string countryCode = "+614";
            const string validChars = "0123456789";
            var number = new StringBuilder(countryCode);
            var random = new Random();

            for (int i = 0; i < 8; i++)
            {
                number.Append(validChars[random.Next(validChars.Length)]);
            }

            return number.ToString();
        }
    }
}