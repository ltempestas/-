using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
//using System.Collections.Generic.Dictationary;

namespace ReadWriteUserToFile
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileFolderPath = Path.GetTempPath();

            MyAppContext myAppContext = new MyAppContext();
            myAppContext.SetDbPath(Path.Combine(fileFolderPath, "my_test_db/"));

            bool isWork = true;

            const string allCommands = @"0 - Вывести всех авторов
1 - Вывести все альбомы
2 - Добавить нового автора
3 - Добавить новый альбом
4 - Удалить автора
5 - Удалить альбом
6 - Выход из программы
------------------";

            while (isWork)
            {
                Console.WriteLine(allCommands);

                string inputCommandStr = Console.ReadLine();

                int inputCommand = 0;

                try
                {
                    inputCommand = int.Parse(inputCommandStr);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Нет такой команды");
                }

                switch (inputCommand)
                {
                    case 0:
                        {
                            var allAuthors = myAppContext.Authors;
                            if (allAuthors.Count == 0) Console.WriteLine("Пока еще нет людей");
                            foreach (var author in allAuthors)
                                Console.WriteLine(author);
                            break;
                        }
                    case 1:
                        {
                            var allAlbums = myAppContext.Albums;
                            if (allAlbums.Count == 0) Console.WriteLine("Пока еще нет альбомов");
                            foreach (var album in allAlbums) Console.WriteLine(album);
                            break;
                        }
                    case 2:
                        {
                            Console.WriteLine("Введите имя:");
                            string name = Console.ReadLine();

                            Console.WriteLine("Введите возраст:");
                            string age = Console.ReadLine();

                            Author newAuthor = new Author(name, age);

                            myAppContext.Authors.Add(newAuthor);

                            myAppContext.SaveChanges();
                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("Введите имя автора:");
                            string name = Console.ReadLine();

                            Console.WriteLine("Введите возраст:");
                            string age = Console.ReadLine();

                            Console.WriteLine("Введите название альбома:");
                            string albumName = Console.ReadLine();

                            Author newAuthor = new Author(name, age);
                            Album newAlbum = new Album(name, age, albumName);

                            myAppContext.Authors.Add(newAuthor);
                            myAppContext.Albums.Add(newAlbum);

                            myAppContext.SaveChanges();
                            break;
                        }
                    case 4:
                        {
                            Console.WriteLine("Введите ID автора:");

                            string idStr = Console.ReadLine();
                            int id = GetIntFromString(idStr);

                            if (id == 0) Console.WriteLine("Нет такого Id");
                            else
                            {
                                try
                                {
                                    Author userForDeletion = myAppContext.Authors.FirstOrDefault(x => x.Id == id);
                                    myAppContext.Authors.Remove(userForDeletion);
                                    myAppContext.SaveChanges();


                                }
                                catch (Exception ex) { Console.WriteLine("Ошибка\n " + ex.Message); }
                            }
                            break;
                        }
                    case 5:
                        {
                            Console.WriteLine("Введите ID альбома:");

                            string idStr = Console.ReadLine();
                            int id = GetIntFromString(idStr);

                            if (id == 0) Console.WriteLine("Нет такого Id");
                            else
                            {
                                try
                                {
                                    Album userForDeletion = myAppContext.Albums.FirstOrDefault(x => x.Id == id);
                                    myAppContext.Albums.Remove(userForDeletion);
                                    myAppContext.SaveChanges();


                                }
                                catch (Exception ex) { Console.WriteLine("Ошибка\n " + ex.Message); }
                            }
                            break;
                        }
                    case 6:
                        {

                            isWork = false;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Нет такой команды");
                            break;
                        }
                }
            }

        }

        static int GetIntFromString(string inputStr)
        {
            int input = 0;
            try
            {
                input = int.Parse(inputStr);
            }
            catch (FormatException)
            {
            }
            return input;
        }
    }

    class Author : IDbElement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }


    public Author(string name, string age)
        {
            Name = name;
            Age = age;
        }

        public override string ToString()
        {
            return $"{Name} {Age}";
        }
    }

    class Album : Author
    {
        public int Id { get; set; }
        public string AlbumName { get; set; }

        public Album(string name, string age, string albumName) : base(name, albumName)
        {
            AlbumName = albumName;
        }
    }

    abstract class MyContext
    {
        protected string DB_PATH { get; set; }

        string fileFormat = ".mydb";

        public virtual void SetDbPath(string db_path)
        {
            DB_PATH = db_path;

            Directory.CreateDirectory(db_path);

            GetValues();
        }

        public void SaveChanges()
        {
            var allDBsAsProps = GetProperties();

            foreach (var set in allDBsAsProps)
            {
                MyDbSet dbSet = set.GetValue(this) as MyDbSet;
                string serialized = JsonConvert.SerializeObject(dbSet);

                string dbPath = Path.Combine(DB_PATH, set.Name + fileFormat);

                File.WriteAllText(dbPath, serialized);

                dbSet.SaveChanges();
            }
        }

        protected IList<PropertyInfo> GetProperties()
        {
            return this.GetType().GetProperties().Where(x => x.PropertyType.BaseType.Name == nameof(MyDbSet)).ToList();
        }

        private void GetValues()
        {
            var allDBsAsProps = GetProperties();
            foreach (var set in allDBsAsProps)
            {
                string dbPath = Path.Combine(DB_PATH, set.Name + fileFormat);
                MyDbSet dbSet = set.GetValue(this) as MyDbSet;

                if (File.Exists(dbPath))
                {
                    string serialized = File.ReadAllText(dbPath);
                    dbSet.Fill(serialized);
                }
            }
        }
    }

    class MyAppContext : MyContext
    {
        public MyDbSet<Author> Authors { get; set; } = new MyDbSet<Author>();
        public MyDbSet<Album> Albums { get; set; } = new MyDbSet<Album>();

        public IEnumerable<string> GetAllProps()
        {
            var allDBs = GetProperties();
            return allDBs.Select(x => x.Name);
        }
    }

    interface IDbElement
    {
        int Id { get; set; }
    }

    interface IMyDbSet
    {
        void SaveChanges();
    }

    abstract class MyDbSet : IMyDbSet
    {
        public bool IsChanged { get; set; }

        public void SaveChanges()
        {
            IsChanged = false;
        }

        public MyDbSet()
        {
            IsChanged = false;
        }

        public abstract void Fill(string data);
    }
    class MyDbSet<T> : MyDbSet, IEnumerable<T> where T : IDbElement
    {
        private List<T> _innerList = new List<T>();

        public int Count => _innerList.Count;

        public void Add(T item)
        {
            item.Id = (_innerList.LastOrDefault()?.Id ?? 0) + 1;
            _innerList.Add(item);

            IsChanged = true;
        }

        public void Clear()
        {
            _innerList.Clear();
            IsChanged = true;
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }
        public bool Remove(T item)
        {
            T itemForRemoved = _innerList.FirstOrDefault(x => x.Id == item.Id);
            IsChanged = true;
            return _innerList.Remove(itemForRemoved);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override void Fill(string data)
        {
            var deserialized = JsonConvert.DeserializeObject<List<T>>(data);
            _innerList = deserialized;
        }
    }
}







//namespace JsonStorage
//{
//    class Program
//    {
//        public static void Main(string[] args)
//        {

//            Dictionary<string, string> authors = new Dictionary<string, string> ();
//            {
//                authors.Add("1", "John");
//                authors.Add("2", "Sam");

//                string json = System.Text.Json.JsonSerializer.Serialize(authors);
//                Console.WriteLine(json);
//            }

//            Dictionary<string, string> albums = new Dictionary<string, string>();
//            {
//                albums.Add("John", "Yellow");
//                albums.Add("Sam", "Love");

//            string json = System.Text.Json.JsonSerializer.Serialize(albums);
//            Console.WriteLine(json);

//            };

//            string allCommands = "\n0 - Вывести всех \n1 - Добавить нового \n2 - Удалить \n3 - Выход из программы \n------------------";
//            while (true)
//            {
//                Console.WriteLine(allCommands);
//                string inputCommandStr = Console.ReadLine();

//                int inputCommand = 0;
//                switch (inputCommand)
//                {
//                    case 0:
//                        {
//                           // var allUsers = myAppContext.Users;
//                           // if (allUsers.Count == 0) Console.WriteLine("Пока еще нет людей");
//                            foreach (var album in albums) 
//                                Console.WriteLine(album);
//                            break;
//                        }
//                    case 1:
//                        {
//                            Console.WriteLine("Введите id автора:");
//                            string id = Console.ReadLine();

//                            Console.WriteLine("Введите никнейм автора:");
//                            string nickname = Console.ReadLine();

//                            Console.WriteLine("Введите Название альбома:");
//                            string albumname = Console.ReadLine();

//                            Author newAuthor = new Author(id, nickname);
//                            Album newAlbum = new Album(albumname, id);

//                            break;
//                        }
//                    //case 2:
//                    //    {
//                    //        Console.WriteLine("Введите ID:");

//                    //        string idStr = Console.ReadLine();
//                    //        int id = GetStringFromString(idStr);

//                    //        if (id == 0) Console.WriteLine("Нет такого Id");
//                    //        else
//                    //        {
//                    //            try
//                    //            {
//                    //                User userForDeletion = myAppContext.Users.FirstOrDefault(x => x.Id == id);
//                    //                myAppContext.Users.Remove(userForDeletion);
//                    //                myAppContext.SaveChanges();

//                    //                Console.WriteLine("Успешно");
//                    //            }
//                    //            catch (Exception ex) { Console.WriteLine("Ошибка\n " + ex.Message); }
//                    //        }
//                    //        break;
//                    //    }
//                    case 3:
//                        {

//                            break;
//                        }
//                    default:
//                        {
//                            Console.WriteLine("Нет такой команды");
//                            break;
//                        }
//                }
//            }




//            //var albums = new Dictionary<int, Album>();
//            //albums.Add(album.Number, album);

//            //var json = JsonSerializer.Serialize(albums);
//            //Console.WriteLine(json);
//            //var loaded = JsonSerializer.Deserialize <Dictionary<int, Album>>(json);
//            //Console.WriteLine(loaded.Count);




//        }
//    }
//     //   private static void SaveToDB();
//     //   private static void DeleteFromDB();
//     //   private static void Read();
//       // private static void ReadAllFromDb();

//}

//public class Dict
//{
//    //public static string ToDict(string line)
//    //{
//    //    return albums[line];

//    //}
//    //public static
//    //Dictionary<int, string> authors = new Dictionary<int, string>
//    //{
//    //    authors.Add
//    //};

//    //Dictionary<string, string> albums = new Dictionary<string, string>
//    //{
//    //    ["John"] = "Cuphead",
//    //    ["John"] = "Love",
//    //    ["Sam"] = "Young",

//    //};
//}



//public class Author
//    { 
//        public string ID { get; set; }
//        public string Nickname { get; set; }
//        public Author(string id, string nickname)
//        {
//            ID = id;
//            Nickname = nickname;
//        }
//        public override string ToString()
//        {
//            return $"{ID} {Nickname}";
//        }
//}
//public class Album
//    {
//      //  public int Number { get; set; }
//        public string AlbumName { get; set; }
//        public Author Author { get; set; }
//        public Album(string albumname, string author)
//        {
//            AlbumName = albumname;


//        }
//        public override string ToString()
//        {
//             return $"{AlbumName} {Author}";
//        }

//}


