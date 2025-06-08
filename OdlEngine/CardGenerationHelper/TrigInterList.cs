using ODLGameEngine;

namespace CardGenerationHelper
{
    public partial class TrigInterList : UserControl
    {
        TrigOrInter trigInter = TrigOrInter.TRIGGER;
        public TrigInterList()
        {
            InitializeComponent();
            RefreshUi();
            ClearPanel();
        }
        void ClearPanel()
        {
            int panelsToRemove = TriginterEffectsPanel.Controls.Count - 1;
            for (int i = 0; i < panelsToRemove; i++) // -1 because last one is the add button
            {
                TriginterEffectsPanel.Controls.RemoveAt(0); // Remove until the button
            }
            TriginterEffectsPanel.Controls.SetChildIndex(AddButton, 0);
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
            TrigInterLabel.Text = trigInter switch
            {
                TrigOrInter.TRIGGER => "Triggers",
                TrigOrInter.INTERACTION => "Interactions",
                _ => throw new ArgumentException("No other type"),
            };
        }
        public void RequestEffectDeletion(TriginterEffects trigInterEffects)
        {
            int index = TriginterEffectsPanel.Controls.IndexOf(trigInterEffects); // What index is it?
            // Remove it from the panel
            TriginterEffectsPanel.Controls.RemoveAt(index);
            // Dispose of the control to free memory
            trigInterEffects.Dispose();
        }
        private void AddButton_Click(object sender, EventArgs e)
        {
            AddTriginterEffects(new TriginterEffects());
        }
        void AddTriginterEffects(TriginterEffects box)
        {
            int newIndex = TriginterEffectsPanel.Controls.Count - 1; // Put the control prev to last
            box.SetOwner(this);
            box.SetTrigInterType(trigInter);
            // Add
            TriginterEffectsPanel.Controls.Add(box);
            TriginterEffectsPanel.Controls.SetChildIndex(box, newIndex);
            // Move button
            TriginterEffectsPanel.Controls.SetChildIndex(AddButton, newIndex + 1);
        }
        public Dictionary<InteractionType, List<Effect>> GetInteractionsDict()
        {
            if (trigInter != TrigOrInter.INTERACTION) throw new Exception("This is not an interaction control!");
            Dictionary<InteractionType, List<Effect>> res = new Dictionary<InteractionType, List<Effect>>();
            for (int i = 0; i < TriginterEffectsPanel.Controls.Count - 1; i++) // -1 because last one is the add button
            {
                TriginterEffects effs = (TriginterEffects)TriginterEffectsPanel.Controls[i];
                KeyValuePair<InteractionType, List<Effect>> kvp = effs.GetInteractionEffects();
                res[kvp.Key] = kvp.Value;
            }
            if (res.Count == 0)
            {
                res = null;
            }
            return res;
        }
        public Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>> GetTriggersDict()
        {
            if (trigInter != TrigOrInter.TRIGGER) throw new Exception("This is not a trigger control!");
            Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>> res = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
            for (int i = 0; i < TriginterEffectsPanel.Controls.Count - 1; i++) // -1 because last one is the add button
            {
                TriginterEffects effs = (TriginterEffects)TriginterEffectsPanel.Controls[i];
                Tuple<EffectLocation, KeyValuePair<TriggerType, List<Effect>>> theTuple = effs.GetTriggerEffects();
                if(!res.TryGetValue(theTuple.Item1, out Dictionary<TriggerType, List<Effect>> value))
                {
                    value = new Dictionary<TriggerType, List<Effect>>();
                    res[theTuple.Item1] = value;
                }

                value.Add(theTuple.Item2.Key, theTuple.Item2.Value);
            }
            if (res.Count == 0)
            {
                res = null;
            }
            return res;
        }
        public void SetInteractionDict(Dictionary<InteractionType, List<Effect>> dict)
        {
            if (trigInter != TrigOrInter.INTERACTION) throw new Exception("This is not an interaction control!");
            // Cleans current effects
            RefreshUi();
            ClearPanel();
            // Now, set stuff
            if (dict == null) return;
            foreach (KeyValuePair<InteractionType, List<Effect>> kvp in dict)
            {
                TriginterEffects newBox = new TriginterEffects();
                newBox.SetTrigInterType(trigInter);
                newBox.SetInteractionEffects(kvp);
                AddTriginterEffects(newBox);
            }
        }
        public void SetTriggerDict(Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>> dict)
        {
            if (trigInter != TrigOrInter.TRIGGER) throw new Exception("This is not an trigger control!");
            // Cleans current effects
            RefreshUi();
            ClearPanel();
            // Now, set stuff
            if (dict == null) return;
            foreach (KeyValuePair<EffectLocation, Dictionary<TriggerType, List<Effect>>> kvp1 in dict)
            {
                foreach(KeyValuePair<TriggerType, List<Effect>> kvp2 in kvp1.Value)
                {
                    TriginterEffects newBox = new TriginterEffects();
                    newBox.SetTrigInterType(trigInter);
                    newBox.SetTriggerEffects(kvp1.Key, kvp2);
                    AddTriginterEffects(newBox);
                }
            }
        }
    }
}
