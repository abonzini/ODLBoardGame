using ODLGameEngine;

namespace CardGenerationHelper
{
    public partial class LocationGrid : UserControl
    {
        public event EventHandler Updated;
        readonly List<CheckBox> CheckBoxes = new List<CheckBox>();
        HashSet<int> CurrentTargets = new HashSet<int>();
        public LocationGrid()
        {
            InitializeComponent();

            for (int row = 0; row < CheckboxTable.RowCount; row++)
            {
                for (int col = 0; col < CheckboxTable.ColumnCount; col++)
                {
                    Control control = CheckboxTable.GetControlFromPosition(col, row);

                    if (control is CheckBox checkbox)
                    {
                        checkbox.Checked = false;
                        CheckBoxes.Add(checkbox);
                        checkbox.CheckedChanged += CheckboxChanged;
                    }
                }
            }
        }
        public void SetMode(CardTargetingType type)
        {
            switch (type)
            {
                case CardTargetingType.BOARD:
                    CheckBoxes[0].Show();
                    CheckBoxes[0].Checked = false;
                    for (int i = 1; i < CheckBoxes.Count; i++)
                    {
                        CheckBoxes[i].Checked = false;
                        CheckBoxes[i].Hide();
                    }
                    break;
                case CardTargetingType.LANE:
                    CheckBoxes[0].Show();
                    CheckBoxes[0].Checked = false;
                    CheckBoxes[1].Show();
                    CheckBoxes[1].Checked = false;
                    CheckBoxes[2].Show();
                    CheckBoxes[2].Checked = false;
                    for (int i = 3; i < CheckBoxes.Count; i++)
                    {
                        CheckBoxes[i].Checked = false;
                        CheckBoxes[i].Hide();
                    }
                    break;
                default:
                    for (int i = 0; i < CheckBoxes.Count; i++)
                    {
                        CheckBoxes[i].Checked = false;
                        CheckBoxes[i].Show();
                    }
                    break;
            }
        }
        void CheckboxChanged(object sender, EventArgs e)
        {
            CurrentTargets = new HashSet<int>();
            for (int i = 0; i < CheckBoxes.Count; i++)
            {
                if (CheckBoxes[i].Checked)
                {
                    CurrentTargets.Add(i);
                }
            }
            Updated.Invoke(this, EventArgs.Empty);
        }
        public HashSet<int> GetLocations()
        {
            return CurrentTargets;
        }
        public void SetLocations(HashSet<int> locations)
        {
            locations ??= new HashSet<int>();
            for (int i = 0; i < CheckBoxes.Count; i++)
            {
                if (locations.Contains(i))
                {
                    CheckBoxes[i].Checked = true;
                }
                else
                {
                    CheckBoxes[i].Checked = false;
                }
            }
        }
    }
}
