using Android.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;


namespace EmotionalDiary
{
    public class StatisticsAdapter : BaseAdapter
    {
        private Activity curvedActivity;
        private List<Emotions> items;

        public StatisticsAdapter(Activity curvedActivity, List<Emotions>  items)
        {
            this.curvedActivity = curvedActivity;
            this.items = items;
        }

        public override int Count => items.Count;

        // public override int Count => ;  

        public override Java.Lang.Object GetItem(int position)
        {
            return items[position].Emotion;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = convertView;
            if (v == null)
            {
                v = curvedActivity.LayoutInflater.Inflate(Resource.Layout.griditem, null);
            }
            if (items[position].Id != -1)
            {
                v.FindViewById<TextView>(Resource.Id.ename).Text = items[position].Emotion;
                v.FindViewById<TextView>(Resource.Id.edate).Text = items[position].ReportDate.ToString("yyyy-MM-dd HH:mm:ss");
                v.FindViewById<TextView>(Resource.Id.edescription).Text = items[position].Description;
            }
            else
            {
                v.FindViewById<TextView>(Resource.Id.ename).Text = v.Resources.GetText(Resource.String.msg_emotion);
                v.FindViewById<TextView>(Resource.Id.edate).Text = v.Resources.GetText(Resource.String.msg_emotion_date);
                v.FindViewById<TextView>(Resource.Id.edescription).Text = v.Resources.GetText(Resource.String.msg_emotion_description);
            }
            return v;
        }
    }
}