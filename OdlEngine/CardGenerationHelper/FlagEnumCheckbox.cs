using ODLGameEngine;

namespace CardGenerationHelper
{
    // Pipeline to add the enum, just add an extra check for the type you're interested in
    public partial class FlagEnumCheckbox : UserControl
    {
        public FlagEnumCheckbox()
        {
            InitializeComponent();
        }
        public void SetEnum(Type theEnumType)
        {
            int numberOfFlags = theEnumType switch
            {
                Type t when t == typeof(EntityOwner) => 2,
                Type t when t == typeof(EntityType) => 4,
                _ => throw new NotImplementedException("Invalid flag enum")
            };
            CheckboxesPanel.Controls.Clear(); // Clear list just in case
            for (int i = 0; i < numberOfFlags; i++) // Add checkboxes one by one
            {
                int flag = 1 << i;
                CheckBox checkBox = new CheckBox
                {
                    Text = Enum.GetName(theEnumType, flag),
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter,
                    CheckAlign = ContentAlignment.BottomCenter // Moves the checkbox below the text
                };
                CheckboxesPanel.Controls.Add(checkBox);
            }
        }
        public int GetEnumValue()
        {
            int result = 0;
            for (int i = 0; i < CheckboxesPanel.Controls.Count; i++)
            {
                int flag = 1 << i;
                if (((CheckBox)CheckboxesPanel.Controls[i]).Checked)
                {
                    result |= flag;
                }
            }
            return result;
        }
        public void Clear()
        {
            for (int i = 0; i < CheckboxesPanel.Controls.Count; i++)
            {
                ((CheckBox)CheckboxesPanel.Controls[i]).Checked = false;
            }
        }
        public void SetFlags(int flags)
        {
            for (int i = 0; i < CheckboxesPanel.Controls.Count; i++)
            {
                int flag = 1 << i;
                ((CheckBox)CheckboxesPanel.Controls[i]).Checked = (flag & flags) != 0;
            }
        }
    }
}
