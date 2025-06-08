using ODLGameEngine;
using System.Data;

namespace CardGenerationHelper
{
    public enum TrigOrInter
    {
        TRIGGER,
        INTERACTION
    }
    public partial class TriginterEffects : UserControl
    {
        TrigInterList owner = null;
        TrigOrInter trigInter = TrigOrInter.TRIGGER;
        public TriginterEffects()
        {
            InitializeComponent();
            RefreshUi();
            ClearPanel();
        }
        void ClearPanel()
        {
            int panelsToRemove = EffectsPanel.Controls.Count - 1;
            for (int i = 0; i < panelsToRemove; i++) // -1 because last one is the add button
            {
                EffectsPanel.Controls.RemoveAt(0); // Remove until the button
            }
            EffectsPanel.Controls.SetChildIndex(AddButton, 0);
        }
        public void SetOwner(TrigInterList owner)
        {
            // Sets ownership
            this.owner = owner;
        }
        public void SetTrigInterType(TrigOrInter trigInter)
        {
            if (this.trigInter != trigInter)
            {
                this.trigInter = trigInter;
                RefreshUi();
            }
        }
        void RefreshUi()
        {
            EventTypeComboBox.Items.Clear();
            switch (trigInter)
            {
                case TrigOrInter.TRIGGER:
                    EventTypeComboBox.Items.AddRange(Enum.GetValues(typeof(TriggerType)).Cast<object>().ToArray());
                    TriggerLocationComboBox.Items.AddRange(Enum.GetValues(typeof(EffectLocation)).Cast<object>().ToArray());
                    break;
                case TrigOrInter.INTERACTION:
                    EventTypeComboBox.Items.AddRange(Enum.GetValues(typeof(InteractionType)).Cast<object>().ToArray());
                    TriggerLocationComboBox.Hide();
                    break;
                default:
                    throw new ArgumentException("No other type");
            }
            EventTypeComboBox.SelectedIndex = 0;
        }
        public void RequestEffectDeletion(EffectBox box)
        {
            int index = EffectsPanel.Controls.IndexOf(box); // What index is it?
            // Remove it from the panel
            EffectsPanel.Controls.RemoveAt(index);
            // Dispose of the control to free memory
            box.Dispose();
        }
        // Need to create new effect!
        private void AddButton_Click(object sender, EventArgs e)
        {
            AddNewEffectBox(new EffectBox());
        }
        void AddNewEffectBox(EffectBox box)
        {
            int newIndex = EffectsPanel.Controls.Count - 1; // Put the control prev to last
            box.SetOwner(this);
            // Add
            EffectsPanel.Controls.Add(box);
            EffectsPanel.Controls.SetChildIndex(box, newIndex);
            // Move button
            EffectsPanel.Controls.SetChildIndex(AddButton, newIndex + 1);
        }
        private List<Effect> GetEffects()
        {
            List<Effect> ret = new List<Effect>();
            for (int i = 0; i < EffectsPanel.Controls.Count - 1; i++) // -1 because last one is the add button
            {
                EffectBox box = (EffectBox)EffectsPanel.Controls[i];
                ret.Add(box.GetEffect());
            }
            return ret;
        }
        public KeyValuePair<InteractionType, List<Effect>> GetInteractionEffects()
        {
            if (trigInter != TrigOrInter.INTERACTION) throw new Exception("This is not an interaction control!");
            List<Effect> effects = GetEffects();
            return new KeyValuePair<InteractionType, List<Effect>>((InteractionType)EventTypeComboBox.SelectedItem, effects);
        }
        public Tuple<EffectLocation, KeyValuePair<TriggerType, List<Effect>>> GetTriggerEffects()
        {
            if (trigInter != TrigOrInter.TRIGGER) throw new Exception("This is not a trigger control!");
            List<Effect> effects = GetEffects();
            return new Tuple<EffectLocation, KeyValuePair<TriggerType, List<Effect>>>((EffectLocation)TriggerLocationComboBox.SelectedItem, new KeyValuePair<TriggerType, List<Effect>>((TriggerType)EventTypeComboBox.SelectedItem, effects));
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            owner?.RequestEffectDeletion(this);
        }
        public void SetInteractionEffects(KeyValuePair<InteractionType, List<Effect>> kvp)
        {
            if (trigInter != TrigOrInter.INTERACTION) throw new Exception("This is not an interaction control!");
            // Cleans current effects
            RefreshUi();
            ClearPanel();
            // Now, set stuff
            EventTypeComboBox.SelectedItem = kvp.Key;
            foreach (Effect effect in kvp.Value)
            {
                EffectBox newBox = new EffectBox();
                newBox.SetEffect(effect);
                AddNewEffectBox(newBox);
            }
        }
        public void SetTriggerEffects(EffectLocation location, KeyValuePair<TriggerType, List<Effect>> kvp)
        {
            if (trigInter != TrigOrInter.TRIGGER) throw new Exception("This is not a trigger control!");
            // Cleans current effects
            RefreshUi();
            ClearPanel();
            // Now, set stuff
            TriggerLocationComboBox.SelectedItem = location;
            EventTypeComboBox.SelectedItem = kvp.Key;
            foreach (Effect effect in kvp.Value)
            {
                EffectBox newBox = new EffectBox();
                newBox.SetEffect(effect);
                AddNewEffectBox(newBox);
            }
        }
    }
}
