namespace GTServer.Resources
{
    public class Dialog
    {
        public string Result { get; set; } = "";
        public Dialog AddButton(string name,string text)
        {
            Result += "\nadd_button|" + name + "|" + text + "|noflags|0|0|";
            return this;
        }
        public Dialog AddTextInput(string name, string text, string textInside, int length)
        {
            Result += "\nadd_text_input|" + name + "|" + text + "|" + textInside + "|" + length + "|";
            return this;
        }
        public Dialog EndDialog(string dialogName,string cancelText, string okText)
        {
            Result += "\nend_dialog|" + dialogName + "|" + cancelText + "|" + okText + "|";
            return this;
        }
        public Dialog AddSmallLabel(string text)
        {
            Result += "\nadd_label|small|" + text + "|left|0|";
            return this;
        }
        public Dialog AddBigLabel(string text)
        {
            Result += "\nadd_label|big|" + text + "|left|0|";
            return this;
        }
        public Dialog AddSmallSpacer()
        {
            Result += "\nadd_spacer|small|";
            return this;
        }
        public Dialog AddBigSpacer()
        {
            Result += "\nadd_spacer|big|";
            return this;
        }
        public Dialog AddLabelWithIcon(string text,int itemsId,bool small)
        {
            if (small)
            {
                Result += "\nadd_label_with_icon|small|" + text + "|left|" + itemsId + "|";
            }
            else
            {
                Result += "\nadd_label_with_icon|big|" + text + "|left|" + itemsId + "|";
            }
            return this;
        }
        public Dialog SetDefaultColor()
        {
            Result += "set_default_color|";
            return this;
        }
        public Dialog AddButtonWithIcon(string buttonName,string text, int itemsId)
        {
            Result += "\nadd_button_with_icon|" + buttonName + "|" + text + "|left|" + itemsId + "||";
            return this;
        }
    }
}
