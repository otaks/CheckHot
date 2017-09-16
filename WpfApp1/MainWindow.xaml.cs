using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1 {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {

        private class Chann {   //番組
            public string Title { get; set; }   //タイトル
            public string AudienceNum { get; set; } //視聴者数
            public string CommentNum { get; set; }  //コメント数
            public string Url { get; set; } //URL(相対)
        }

        private static List<Chann> innerList = new List<Chann>();

        public MainWindow() {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e) {
            innerList.Clear();
            listBox.Items.Clear();
            await UpdateList("http://live.nicovideo.jp/recent?tab=common#/start_time/desc/1");
            await UpdateList("http://live.nicovideo.jp/recent?tab=common#/start_time/desc/2"); 

            innerList.ForEach(item => {
                listBox.Items.Add(item.Title + "\n" + item.AudienceNum + "\n" + item.CommentNum);
            });
        }

        private async Task UpdateList(string urlstring) {
            var doc = default(IHtmlDocument);
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(new Uri(urlstring))) {
                var parser = new HtmlParser();
                doc = await parser.ParseAsync(stream);
            }

            var listItems = doc.QuerySelectorAll("#onair_stream_list > ul.user-programs > li").Select(n => {
                var title = n.Attributes["title"].Value;
                var status = n.QuerySelectorAll("a > ul > li");
                var audienceNum = status[0].QuerySelectorAll("span")[1].TextContent;
                var commentNum = status[1].QuerySelectorAll("span")[1].TextContent;
                var url = n.QuerySelector("a").Attributes["href"].Value;
                if ((audienceNum != "--") && (int.Parse(audienceNum) >= 25) && (!IsExist(title))) {
                    return new Chann { Title = title, AudienceNum = audienceNum, CommentNum = commentNum, Url = url };
                } else {
                    return new Chann { Title = "@", AudienceNum = "@", CommentNum = "@", Url = "@" };
                }
            });

            listItems.ToList().ForEach(item => {
                if (item.Title != "@") {
                    innerList.Add(item);
                }
                });
        }

        private bool IsExist(string title) {
            foreach (string i in listBox.Items) {
                if (i.Contains(title))
                    return true;
            }
            return false;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            int i = ((ListBox)sender).SelectedIndex;
            System.Diagnostics.Process.Start("http://live.nicovideo.jp/" + innerList.ElementAt(i).Url);
        }
    }
}
