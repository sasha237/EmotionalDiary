using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Environment = System.Environment;

namespace EmotionalDiary
{
    [Table("Items")]
    public class Emotions
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public DateTime ReportDate { get; set; }

        [MaxLength(128)]
        public string Emotion { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }
    }
    public class DBWrapper
    {
        SQLiteConnection db = null;
        public DBWrapper()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "emotions.db3");
            db = new SQLiteConnection(dbPath);
            db.CreateTable<Emotions>();
        }

        public void Add(string emotion, DateTime emotionDate, string Description)
        {
            var e = new Emotions();
            e.Emotion = emotion;
            e.ReportDate = emotionDate;
            e.Description = Description;
            db.Insert(e);
        }

        public void DeleteLast()
        {
            var last = GetLast();
            if (last != null)
            {
                db.Delete<Emotions>(last.Id);
            }
        }

        public void DeleteAll()
        {
            db.DeleteAll<Emotions>();
        }

        public Emotions GetLast()
        {
            var last = db.Table<Emotions>().OrderByDescending(x => x.Id).FirstOrDefault();
            return last;
        }

        public List<Emotions> GetAll()
        {
            return db.Table<Emotions>().OrderBy(x => x.Id).ToList();
        }
        public List<Emotions> GetAllWithHeader()
        {
            Emotions e = new Emotions();
            e.Id = -1;
            e.Description = "Description";
            e.Emotion = "Emotion";
            e.ReportDate = DateTime.Now;
            List<Emotions> list = new List<Emotions>();
            list.Add(e);
            list.AddRange(db.Table<Emotions>().OrderBy(x => x.Id).ToList());
            return list;
        }
    }
}