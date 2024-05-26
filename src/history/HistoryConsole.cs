using System.Windows.Controls;
using System.Windows.Documents;

namespace dankeyboard.src.history
{
    public static class HistoryConsole {

        public static void updateConsole(Grid kb, string text) {

            System.Windows.Controls.RichTextBox? historyConsole = kb.FindName("displayHistory") as System.Windows.Controls.RichTextBox;

            // add text to richtextbox
            historyConsole.AppendText(text + "\n");

            // if limit reaches 100
            // TODO: add customization to console, including line limit
            historyConsole.Document.LineHeight = 1;
            int lineCount = historyConsole.Document.Blocks.Count;
            if (lineCount > 100) {
                TextPointer firstLineStart = historyConsole.Document.ContentStart;
                TextPointer firstLineEnd = historyConsole.Document.ContentStart.GetLineStartPosition(1);
                historyConsole.Document.Blocks.Remove(historyConsole.Document.Blocks.FirstBlock);
            }
            historyConsole.ScrollToEnd();

        }
    }
}
