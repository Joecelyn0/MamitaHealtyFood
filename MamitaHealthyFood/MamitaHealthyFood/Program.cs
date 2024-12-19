using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

using System;
using MySql.Data.MySqlClient;

namespace MamitaHealthyFood
{
    class Program
    {
        static Database db = new Database();
        static User currentUser = null;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Keluar");
                Console.Write("Tentukan Pilihan: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        Register();
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Pilihanmu salah, silahkan coba lagi.");
                        break;
                }
            }
        }

        static void Login()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            using (var connection = db.GetConnection())
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM users WHERE username=@username AND password=@password", connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentUser = new User
                        {
                            Id = reader.GetInt32("id"),
                            Username = reader.GetString("username"),
                            IsAdmin = reader.GetBoolean("is_admin")
                        };
                        Console.WriteLine("Login Berhasil, Yey!");
                        MainMenu();
                    }
                    else
                    {
                        Console.WriteLine("Username atau Passwordmu Salah.");
                    }
                }
            }
        }

        static void Register()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            using (var connection = db.GetConnection())
            {
                connection.Open();
                var command = new MySqlCommand("INSERT INTO users (username, password) VALUES (@username, @password)", connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                try
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Registration Berhasil!");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void MainMenu()
        {
            while (true)
            {
                Console.WriteLine("1. Lihat Produk");
                Console.WriteLine("2. Order Produk");
                Console.WriteLine("3. Lihat Pesanan");
                Console.WriteLine("4. Hapus Pesanan");
                Console.WriteLine("5. Keluar");
                Console.Write("Tentukan Pilihanmu: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewProducts();
                        break;
                    case "2":
                        PlaceOrder();
                        break;
                    case "3":
                        ViewOrders();
                        break;
                    case "4":
                        DeleteOrder();
                        break;
                    case "5":
                        currentUser = null;
                        return;
                    default:
                        Console.WriteLine("Pilihanmu salah, silahkan coba lagi.");
                        break;
                }
            }
        }

        static void ViewProducts()
        {
            using (var connection = db.GetConnection())
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM products", connection);
                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("Produk yang tersedia:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt32("id")}. {reader.GetString("name")} - {reader.GetDecimal("price")}");
                    }
                }
            }
        }

        static void PlaceOrder()
        {
            Console.Write("Tulis ID Produk: ");
            int productId = int.Parse(Console.ReadLine());
            Console.Write("Tulis Jumlahnya: ");
            int quantity = int.Parse(Console.ReadLine());

            decimal productPrice = 0;

            using (var connection = db.GetConnection())
            {
                connection.Open();

                // Get product price
                var command = new MySqlCommand("SELECT price FROM products WHERE id=@productId", connection);
                command.Parameters.AddWithValue("@productId", productId);
                productPrice = (decimal)command.ExecuteScalar();

                // Insert order
                command = new MySqlCommand("INSERT INTO orders (user_id, product_id, quantity) VALUES (@userId, @productId, @quantity)", connection);
                command.Parameters.AddWithValue("@userId", currentUser.Id);
                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@quantity", quantity);

                command.ExecuteNonQuery();
                decimal totalPrice = productPrice * quantity;
                Console.WriteLine($"Pesananmu Berhasil! Harga Total: {totalPrice}");
            }
        }

        static void ViewOrders()
        {
            using (var connection = db.GetConnection())
            {
                connection.Open();
                var command = new MySqlCommand("SELECT o.id, p.name, o.quantity, o.order_time, p.price FROM orders o JOIN products p ON o.product_id = p.id WHERE o.user_id = @userId", connection);
                command.Parameters.AddWithValue("@userId", currentUser.Id);

                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("Your Orders:");
                    while (reader.Read())
                    {
                        int orderId = reader.GetInt32("id");
                        string productName = reader.GetString("name");
                        int quantity = reader.GetInt32("quantity");
                        DateTime orderTime = reader.GetDateTime("order_time");
                        decimal productPrice = reader.GetDecimal("price");
                        decimal totalPrice = productPrice * quantity;

                        Console.WriteLine($"Order ID: {orderId}, Product: {productName}, Quantity: {quantity}, Total Price: {totalPrice}, Time: {orderTime}");
                    }
                }
            }
        }

        static void DeleteOrder()
        {
            Console.Write("Tulis ID Produk untuk Menghapus: ");
            int orderId = int.Parse(Console.ReadLine());

            using (var connection = db.GetConnection())
            {
                connection.Open();
                var command = new MySqlCommand("DELETE FROM orders WHERE id=@orderId AND user_id=@userId", connection);
                command.Parameters.AddWithValue("@orderId", orderId);
                command.Parameters.AddWithValue("@userId", currentUser.Id);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Pesanan Berhasil Dibatalkan.");
                }
                else
                {
                    Console.WriteLine("Pesananmu tidak ditemukan. Kamu membutuhkan izin untuk menghapusnya.");
                }
            }
        }
    }
}