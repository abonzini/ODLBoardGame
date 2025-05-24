using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardGenerationHelper
{
    public partial class TrigInterList : UserControl
    {
        TrigOrInter trigInter = TrigOrInter.TRIGGER;
        public TrigInterList()
        {
            InitializeComponent();
            RefreshUi();
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
            switch (trigInter)
            {
                case TrigOrInter.TRIGGER:
                    TrigInterLabel.Text = "Triggers";
                    break;
                case TrigOrInter.INTERACTION:
                    TrigInterLabel.Text = "Interactions";
                    break;
                default:
                    throw new ArgumentException("No other type");
            }
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
            int newIndex = TriginterEffectsPanel.Controls.Count - 1; // Put the control prev to last
            TriginterEffects newTrigInterEffects = new TriginterEffects();
            newTrigInterEffects.SetOwner(this);
            newTrigInterEffects.SetTrigInterType(trigInter);
            // Add
            TriginterEffectsPanel.Controls.Add(newTrigInterEffects);
            TriginterEffectsPanel.Controls.SetChildIndex(newTrigInterEffects, newIndex);
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
            return res;
        }
        public Dictionary<TriggerType, List<Effect>> GetTriggersDict()
        {
            if (trigInter != TrigOrInter.INTERACTION) throw new Exception("This is not an interaction control!");
            Dictionary<TriggerType, List<Effect>> res = new Dictionary<TriggerType, List<Effect>>();
            for (int i = 0; i < TriginterEffectsPanel.Controls.Count - 1; i++) // -1 because last one is the add button
            {
                TriginterEffects effs = (TriginterEffects)TriginterEffectsPanel.Controls[i];
                KeyValuePair<TriggerType, List<Effect>> kvp = effs.GetTriggerEffects();
                res[kvp.Key] = kvp.Value;
            }
            return res;
        }
    }
}
