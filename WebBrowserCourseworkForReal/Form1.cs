﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace WebBrowserCourseworkForReal
{
    public partial class Form1 : Form
    {
        // This variable holds the C# Object converted from the JSON. Please check the JSON schema to know the structure of this variable.
        static dynamic userdata;

        // This variable is used to track the current index the user is at in the userdata.history list
        static int userPosition;

        /*
         *  From1 is a constructer responsible for the follwing:
         *  1. Calls initalize component from From1.Designer.cs
         *  2. Check if file exists for userdata.json, which contains all user settings for the browser
         *  2.1 Create and write to file if file doesn't exists
         *  2.2 Read json data to the userdata variable
         *  3. Load up homepage
         *  
         */
        public Form1()
        {
            InitializeComponent();

            if (!File.Exists("userdata.json"))
            {
                File.WriteAllText("userdata.json", "{\"homepage\":\"http://www.google.com\",\"bookmarks\":[],\"history\":[]}");
            }
            
            using (StreamReader r = new StreamReader("userdata.json"))
            {
                string jsonText = r.ReadToEnd();
                r.Close();
                userdata = JsonConvert.DeserializeObject(jsonText);
                userPosition = userdata.history.Count - 1;
            }

            textBoxURL.Text = userdata.homepage;
        }

        // Go button (next to URL Text box)
        private void button1_Click(object sender, EventArgs e)
        {
            loadResponseToTextBoxFromURL(true);
        }

        private void setAsHomePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userdata["homepage"] = textBoxURL.Text;
            MessageBox.Show("Home page has been set to " + userdata["homepage"]);
        }

        private void buttonAddTab_click(object sender, EventArgs e)
        {
            addWebPageTab(userdata.homepage.ToString(), false);
        }

        // Remove tab button
        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        // On Web Browser close
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            JsonSerializer seralizer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter("userdata.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                seralizer.Serialize(writer, userdata);
            }
        }

        private void goBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userdata.history.Count > 1)
            {
                textBoxURL.Text = userdata.history[userPosition - 1];
                userPosition -= 1;
                loadResponseToTextBoxFromURL(false);
            }
        }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userdata.history.Clear();
            userPosition = 0;
            goForwardToolStripMenuItem.Enabled = false;
            goBackToolStripMenuItem.Enabled = false;
        }

        private void goForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userPosition != userdata.history.Count-1 && userPosition != 0)
            {
                textBoxURL.Text = userdata.history[userPosition + 1];
                userPosition += 1;
                loadResponseToTextBoxFromURL(false);
            }
        }

        private void goHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxURL.Text = userdata.homepage;
            loadResponseToTextBoxFromURL(true);
        }

        private void viewHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if history is already open
            foreach (TabPage tabpage in tabControl1.TabPages)
            {
                if (tabpage.Text == "History")
                {
                    MessageBox.Show("A History tab is already open");
                    return;
                }
            }

            // Setup tab page
            string title = "History";
            TabPage newTabPage = new TabPage(title);
            tabControl1.TabPages.Add(newTabPage);

            // Set up view control
            ListView listViewHistory = new ListView();
            listViewHistory.Location = new System.Drawing.Point(-4, 0);
            listViewHistory.Name = "listViewHistory";
            listViewHistory.Size = new System.Drawing.Size(1174, 646);
            listViewHistory.UseCompatibleStateImageBehavior = false;
            listViewHistory.View = View.List;
            listViewHistory.ItemActivate += listViewHistory_ItemClick;

            // Load data to list view
            foreach (string item in userdata.history)
            {
                listViewHistory.Items.Add(item);
            }

            // Add control to tabpage
            newTabPage.Controls.Add(listViewHistory);
            tabControl1.SelectedTab = newTabPage;
        }

        private void listViewHistory_ItemClick(Object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            textBoxURL.Text = listView.SelectedItems[0].Text;
            addWebPageTab(textBoxURL.Text, false);
        }

        private void addBookmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userdata.bookmarks.Add(showBookmarkEditDialog(textBoxURL.Text, "Add new bookmark"));
        }

        private void myBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if bookmarks is already open
            foreach (TabPage tabpage in tabControl1.TabPages)
            {
                if(tabpage.Text == "Bookmarks")
                {
                    MessageBox.Show("A Bookmarks tab is already open");
                    return;
                }
            }

            // Setup tab page
            string title = "Bookmarks";
            TabPage newTabPage = new TabPage(title);
            tabControl1.TabPages.Add(newTabPage);

            // Set up view control
            ListView listViewBookmarks = new ListView();
            listViewBookmarks.Location = new System.Drawing.Point(-4, 0);
            listViewBookmarks.Name = "listViewBookmarks";
            listViewBookmarks.Size = new System.Drawing.Size(400, 620);
            listViewBookmarks.UseCompatibleStateImageBehavior = false;
            listViewBookmarks.View = View.Details;
            listViewBookmarks.ItemActivate += listViewBookmarks_ItemClick;
            listViewBookmarks.Columns.Add("Name", 200);
            listViewBookmarks.Columns.Add("URL", 200);
            newTabPage.Controls.Add(listViewBookmarks);

            int itemTop = 20;
            int itemIndex = 0;

            // Load data to list view
            foreach (dynamic item in userdata.bookmarks)
            {
                ListViewItem listItem = new ListViewItem((string)item.name);
                listItem.SubItems.Add((string)item.url);
                Button itemEditButton = new Button() { Text = "Edit Item "+itemIndex, Left = 400, Width = 100, Height = 20, Top = itemTop };
                itemEditButton.Click += bookmarkItemEdit_Click;
                Button itemRemoveButton = new Button() { Text = "Remove Item "+itemIndex, Left = 500, Width = 100, Height = 20, Top = itemTop };
                itemRemoveButton.Click += bookmarkItemRemove_Click;
                itemTop += 20;
                itemIndex++;
                newTabPage.Controls.Add(itemEditButton);
                newTabPage.Controls.Add(itemRemoveButton);
                listViewBookmarks.Items.Add(listItem);
            }

            // Add control to tabpage
            tabControl1.SelectedTab = newTabPage;
        }

        private void bookmarkItemEdit_Click(Object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int itemIndex = Int32.Parse(button.Text.Substring(button.Text.Length-1));
            userdata.bookmarks[itemIndex] = showBookmarkEditDialog((string)userdata.bookmarks[itemIndex].url,"Edit Bookmark",(string)userdata.bookmarks[itemIndex].name);
            button2.PerformClick();
            myBookmarksToolStripMenuItem.PerformClick();
        }

        private void bookmarkItemRemove_Click(Object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int itemIndex = Int32.Parse(button.Text.Substring(button.Text.Length - 1));
            userdata.bookmarks.RemoveAt(itemIndex);
            button2.PerformClick();
            myBookmarksToolStripMenuItem.PerformClick();
        }

        private void listViewBookmarks_ItemClick(Object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            textBoxURL.Text = listView.SelectedItems[0].SubItems[1].Text;
            addWebPageTab(textBoxURL.Text, false);
        }

        private void editHomeURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userdata.homepage = ShowEditDialog((string)userdata.homepage, "Edit Homepage");
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                buttonGO.PerformClick();
            }

            if(e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control)
            {
                buttonAddTab.PerformClick();
                buttonGO.PerformClick();
            }
        }

        // REUSABLE FUNCTIONS BELOW -- REUSABLE FUNCTIONS BELOW 

        /*
         * toggleNavigationButtons()
         * This function is responsible for toggling (enable/disable) navigation buttons buttonForward and buttonBackward, based on the current position of the user in the history.
         * 
         * Parameters: None
         * Returns: None
         */
        private void toggleNavigationButtons()
        {
            if (userdata.history.Count <= 1)
            {
                goForwardToolStripMenuItem.Enabled = false;
                goBackToolStripMenuItem.Enabled = false;
            }
            else
            {
                goForwardToolStripMenuItem.Enabled = userPosition == userdata.history.Count - 1 ? false : true;
                goBackToolStripMenuItem.Enabled = userPosition == 1 ? false : true;
            }
        }

        /*
         * This function is responsible for displaying an error message in a given RichTextBox control.
         * 
         * Parameters:
         * selectedRichTextBox (RichTextBox) : The error message is displayed in the provided RichTextBox control
         * Returns: None
         */
        private void invalidHTTPResponse(RichTextBox selectedRichTextBox)
        {
            // no http status code available
            selectedRichTextBox.Text = "No Response Code\n--------------\nUnable to make a valid HTTP request, please check the URL";
            updateStatusCode("---", Color.Gray);
        }

        /*
         * updateStatusCode(string text, Color color)
         * This function is responsible for updating the labelStatusCode to the given text and color. It is usually used to display the status code from HTTP response. 
         * 
         * Parameters:
         * text (string) : The Text to display
         * color (Color) : Color to set for the text
         * Returns: None
         */
        private void updateStatusCode(string text, Color color)
        {
            labelStatusCode.Text = text;
            labelStatusCode.ForeColor = color;
        }

        /*
         * loadResponseToTextBoxFromURL(bool recordToHistory)
         * This function is responsible for doing HTTP request and displaying the response in a richTextBox of a selected tab. Depending on the status of the request, this function will automatically update the text of the richTextBox (eg. Loading, success, invalid, etc).
         * 
         * Parameters:
         * recordToHistory (bool) : On a successful response, should the URL be stored in userdata.history
         * Returns: None
         *
         */
        private async void loadResponseToTextBoxFromURL(bool recordToHistory)
        {
            Uri uriResult;
            bool isUri = Uri.TryCreate(textBoxURL.Text, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;

            RichTextBox selectedRichTextBox;

            if (tabControl1.TabCount > 0)
            {
                if (tabControl1.SelectedTab.Controls[0] is RichTextBox)
                {
                    selectedRichTextBox = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                }
                else
                {
                    MessageBox.Show("Please use a different tab");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please add a new tab");
                return;
            }

            if (isUri)
            {
                var request = WebRequest.Create(textBoxURL.Text);

                selectedRichTextBox.Text = "Loading...";

                HttpStatusCode responseStatusCode;

                try
                {
                    // Set current tab name to URL name
                    tabControl1.SelectedTab.Text = textBoxURL.Text;

                    // Setup response object
                    var response = (HttpWebResponse)await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);

                    // Get the stream associated with the response.
                    Stream receiveStream = response.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                    // Get the status code
                    responseStatusCode = response.StatusCode;

                    // Send the stream to the RichTextBox1
                    selectedRichTextBox.Text = "Response Code: " + (int)responseStatusCode + "\n-----------------\n" + readStream.ReadToEnd();
                    updateStatusCode(response.StatusCode.ToString(), Color.Green);

                    // Close response and stream
                    response.Close();
                    readStream.Close();

                    // Record history
                    if (recordToHistory)
                    {
                        if (userPosition != userdata.history.Count - 1 || userPosition != 0)
                        {
                            int j = userdata.history.Count;
                            for (int i = userdata.history.Count - 1; i > userPosition; i--)
                            {
                                userdata.history[i].Remove();
                            }
                        }
                        userdata.history.Add(textBoxURL.Text);
                        if (userdata.history.Count == 1)
                        {
                            userPosition = 0;
                        }
                        else
                        {
                            userPosition += 1;
                        }
                    }

                    // Update navigation buttons
                    toggleNavigationButtons();
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            selectedRichTextBox.Text = "Response Code: " + (int)response.StatusCode + "\n--------------\nUnable to make a valid HTTP request, please check the URL";
                            updateStatusCode(response.StatusCode.ToString(), Color.Orange);
                        }
                        else
                        {
                            // no http status code available
                            invalidHTTPResponse(selectedRichTextBox);
                        }
                    }
                    else
                    {
                        // no http status code available
                        invalidHTTPResponse(selectedRichTextBox);
                    }
                }
            }
            else
            {
                // In case URI is invalid
                selectedRichTextBox.Text = "'" + textBoxURL.Text + "' is not a valid URL, Please enter a valid URL";
                updateStatusCode("---", Color.Gray);
            }

        }

        /*
         * ShowEditDialog(string text, string caption)
         * This function is responsible for displaying a Prompt with a caption which allows the user to edit the given text.The text is then returned by this function after the user has finished editing the text.Usually used for editing an item from the list (eg.History)
         * 
         * Parameters:
         * text (string) : The Default text to load in the TextBox
         * caption(string) : The text for the Label of the prompt
         * 
         * Returns: string
         */
        public static string ShowEditDialog(string text, string caption)
        {

            // Setup dialog box
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 200;
            prompt.Text = caption;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = caption };
            TextBox inputBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            inputBox.Text = text;
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.ShowDialog();
            return inputBox.Text;
        }

        /*
         * showBookmarkEditDialog(string url, string caption, string name = “My Bookmark”)
         * This function is similar to ShowEditDialog function, but is used in a different context.It is usually used to allow the user to edit a bookmark item(which has a name and a url). This function is responsible for displaying a prompt with a label(caption) which allows the user to edit a url and name.
         * 
         * Parameters:
         * url (string | required) : The URL to display in the first text box
         * caption(string | require) : The text for the label
         * name(string | optional) : The Name to display in the second text box(default is “My Bookmark”)
         * 
         * Returns: dynamic
         */
        public static dynamic showBookmarkEditDialog(string url, string caption, string name = "My Bookmark")
        {

            // Setup dialog box
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 200;
            prompt.Text = caption;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = caption };
            TextBox nameInputBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            TextBox urlInputBox = new TextBox() { Left = 50, Top = 70, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 90 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            urlInputBox.Text = url;
            nameInputBox.Text = name;
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(nameInputBox);
            prompt.Controls.Add(urlInputBox);
            prompt.ShowDialog();
            dynamic bookmarkItem = new Newtonsoft.Json.Linq.JObject();
            bookmarkItem.name = nameInputBox.Text;
            bookmarkItem.url = urlInputBox.Text;
            if (bookmarkItem.name == "")
            {
                bookmarkItem.name = name;
            }
            if (bookmarkItem.url == "")
            {
                bookmarkItem.name = url;
            }
            return bookmarkItem;
        }

        /*
         * addWebPageTab(string url, Boolean record = false)
         * This function is responsible for adding a new tab and loading a web address response inside the newly created tab’s RichTextBox.
         * 
         * Parameters:
         * url (string | required) : The URL to load in the new tab
         * record (boolean | optional) : Whether to add this URL in the history (Default is false)
         * Returns: none
         */
        private void addWebPageTab(string url, Boolean record = false)
        {
            // Set the title
            string title = url;

            // Set up rich text box
            RichTextBox richTextBoxTab = new RichTextBox();
            richTextBoxTab.Location = new System.Drawing.Point(0, 0);
            richTextBoxTab.Name = "richTextBoxTab" + (tabControl1.TabCount + 1).ToString();
            richTextBoxTab.Size = new System.Drawing.Size(1179, 646);

            // Add new tab
            TabPage newTabPage = new TabPage(title);
            tabControl1.TabPages.Add(newTabPage);

            // Add controller
            newTabPage.Controls.Add(richTextBoxTab);
            tabControl1.SelectedTab = newTabPage;

            // Auto-Navigate to url
            textBoxURL.Text = url;
            loadResponseToTextBoxFromURL(record);
        }
    }
}
