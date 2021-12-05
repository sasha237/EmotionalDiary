using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace EmotionalDiary
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Positions positions;
        DBWrapper db;
        const int CREATE_FILE = 1;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            FloatingActionButton fabtable = FindViewById<FloatingActionButton>(Resource.Id.fabtable);
            fabtable.Click += FabTableOnClick;


            View main_view = FindViewById<View>(Resource.Id.imageView1);
            main_view.Touch += MainViewOnClick;

            var serializer = new XmlSerializer(typeof(Positions));

            using (TextReader reader = new StringReader(GetResourceTextFile("EmotionalDiary.Resources.values.positions.xml")))
            {
                positions = (Positions)serializer.Deserialize(reader);
            }

            db = new DBWrapper();
        }

        void ShowMessage(int res)
        {
            var msg = Resources.GetText(res);
            View view = (View)FindViewById<View>(Resource.Id.imageView1);
            Snackbar.Make(view, msg, Snackbar.LengthLong).SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public string GetResourceTextFile(string filename)
        {
            string result = string.Empty;

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream(filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            switch (id)
            {
                case Resource.Id.action_delete_last:
                    using (var builder = new Android.App.AlertDialog.Builder(this))
                    {
                        builder.SetTitle(Resources.GetText(Resource.String.msg_warning)).SetMessage(Resources.GetText(Resource.String.msg_delete_last_question))
                            .SetPositiveButton(Resources.GetText(Resource.String.msg_yes),
                                (s, e) => {
                                    db.DeleteLast();
                                    ShowMessage(Resource.String.msg_deleted);
                                })
                            .SetNegativeButton(Resources.GetText(Resource.String.msg_no), (s, e) => {
                                ShowMessage(Resource.String.msg_cancelled);
                            }).Show();
                    }
                    return true;
                case Resource.Id.action_delete_all:
                    using (var builder = new Android.App.AlertDialog.Builder(this))
                    {
                        builder.SetTitle(Resources.GetText(Resource.String.msg_warning)).SetMessage(Resources.GetText(Resource.String.msg_clear_db_question))
                            .SetPositiveButton(Resources.GetText(Resource.String.msg_yes),
                                (s, e) => {
                                    db.DeleteAll();
                                    ShowMessage(Resource.String.msg_database_cleared);
                                })
                            .SetNegativeButton(Resources.GetText(Resource.String.msg_no), (s, e) => {
                                ShowMessage(Resource.String.msg_cancelled);
                            }).Show();
                    }
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            var es = db.GetLast();
            if (es == null)
            {
                ShowMessage(Resource.String.msg_nothing_to_save);
                return;
            }
            

            Android.Content.Intent intent = new Android.Content.Intent(Android.Content.Intent.ActionCreateDocument);
            intent.AddCategory(Android.Content.Intent.CategoryOpenable);
            intent.SetType("application/csv");
            intent.PutExtra(Android.Content.Intent.ExtraTitle, "export.csv");

            StartActivityForResult(intent, CREATE_FILE);

        }

        private void FabTableOnClick(object sender, EventArgs eventArgs)
        {
            using (var builder = new Android.App.AlertDialog.Builder(this))
            {
                Android.Widget.GridView input = new Android.Widget.GridView(this);
                //input.NumColumns = 4;
                input.Adapter = new StatisticsAdapter(this, db.GetAllWithHeader());

                Android.Widget.LinearLayout.LayoutParams lp = new Android.Widget.LinearLayout.LayoutParams(
                                      Android.Widget.LinearLayout.LayoutParams.MatchParent,
                                      Android.Widget.LinearLayout.LayoutParams.MatchParent);
                input.LayoutParameters = lp;
                input.FocusedByDefault = true;

                var timeMoment = DateTime.Now;
                builder.SetTitle(Resources.GetText(Resource.String.msg_information))
                    .SetPositiveButton(Resources.GetText(Resource.String.msg_ok), (s, e) => { });

                var myCustomDialog = builder.Create();

                myCustomDialog.SetView(input);

                myCustomDialog.Show();
            }

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Android.Content.Intent? resultData)
        {
            if (requestCode == CREATE_FILE)
            {
                int result = Resource.String.msg_cancelled;
                if (resultCode == Android.App.Result.Ok)
                {
                    // The result data contains a URI for the document or directory that
                    // the user selected.
                    if (resultData != null && resultData.Data != null)
                    {
                        using (var pFD = ContentResolver.OpenFileDescriptor(resultData.Data, "w"))
                        using (var outputSteam = new Java.IO.FileOutputStream(pFD.FileDescriptor))
                        {
                            var es = db.GetAll();
                            string csv = "Id\tEmotion\tReportDate\tDescription\r\n" + String.Join("\r\n", es.Select(x => string.Format("{0}\t\"{1}\"\t{2}\t\"А{3}\"", x.Id, x.Emotion, x.ReportDate.ToString("yyyy-MM-dd HH:mm:ss"), x.Description)).ToArray());

                            var buffer = System.Text.Encoding.UTF8.GetBytes(csv);

                            outputSteam.Write(buffer);
                            outputSteam.Flush();
                            outputSteam.Close();
                            result = Resource.String.msg_saved;
                        }
                    }
                }
                else
                {
                    result = Resource.String.msg_cancelled;
                }
                ShowMessage(result);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void MainViewOnClick(object sender, EventArgs e)
        {
            var view = (sender as Android.Widget.ImageView);
            var my_event = (e as Android.Views.View.TouchEventArgs);
            if (my_event.Event.ActionMasked == MotionEventActions.Up)
            {
                var x = (int)(my_event.Event.GetX() * 100 / view.Width);
                var y = (int)(my_event.Event.GetY() * 100 / view.Height);

                PositionsItemPosition currentposition = null;
                double distance = double.MaxValue;
                foreach(var item in positions.ItemPosition)
                {
                    double cur = Math.Sqrt(Math.Pow(item.x - x, 2) + Math.Pow(item.y - y, 2));
                    if(distance>cur)
                    {
                        distance = cur;
                        currentposition = item;
                    }
                }
                
                using (var builder = new Android.App.AlertDialog.Builder(this))
                {
                    Android.Widget.EditText input = new Android.Widget.EditText(this);
                    Android.Widget.LinearLayout.LayoutParams lp = new Android.Widget.LinearLayout.LayoutParams(
                                          Android.Widget.LinearLayout.LayoutParams.MatchParent,
                                          Android.Widget.LinearLayout.LayoutParams.MatchParent);
                    input.LayoutParameters = lp;
                    input.FocusedByDefault = true;

                    var timeMoment = DateTime.Now;
                    builder.SetTitle(string.Format(Resources.GetText(Resource.String.msg_emotion_context), currentposition.name, timeMoment.ToString("yyyy-MM-dd HH:mm:ss")))
                        .SetMessage(Resources.GetText(Resource.String.msg_write_emotion))
                        .SetPositiveButton(Resources.GetText(Resource.String.msg_ok),
                            (s, e) => {
                                if (!string.IsNullOrEmpty(input.Text))
                                {
                                    db.Add(currentposition.name, timeMoment, input.Text);
                                    ShowMessage(Resource.String.msg_saved);
                                }
                                else
                                {
                                    ShowMessage(Resource.String.msg_nothing_to_save);
                                }
                            })
                        .SetNegativeButton(Resources.GetText(Resource.String.msg_cancel), (s, e) => { });

                    var myCustomDialog = builder.Create();
                    
                    myCustomDialog.SetView(input);
                    
                    myCustomDialog.Show();
                }
                
            }
        }
    }
}
