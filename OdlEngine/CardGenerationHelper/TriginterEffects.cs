using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardGenerationHelper
{
    public enum TrigOrInter
    {
        TRIGGER,
        INTERACTION
    }
    public partial class TriginterEffects : UserControl
    {
        TrigOrInter trigInter = TrigOrInter.TRIGGER;
        public TriginterEffects()
        {
            InitializeComponent();
            RefreshUi();
            EffectsPanel.Controls.SetChildIndex(AddButton, 0);
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
                    EventTypeComboBox.Items.AddRange(Enum.GetValues(typeof(TriggerType)).Cast<object>().ToArray());
                    break;
                case TrigOrInter.INTERACTION:
                    EventTypeComboBox.Items.AddRange(Enum.GetValues(typeof(InteractionType)).Cast<object>().ToArray());
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
            int newIndex = EffectsPanel.Controls.Count - 1; // Put the control prev to last
            EffectBox newBox = new EffectBox();
            newBox.SetOwner(this);
            // Add
            EffectsPanel.Controls.Add(newBox);
            EffectsPanel.Controls.SetChildIndex(newBox, newIndex);
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
        private KeyValuePair<InteractionType, List<Effect>> GetInteractionEffects()
        {
            if (trigInter != TrigOrInter.INTERACTION) throw new Exception("This is not an interaction control!");
            List<Effect> effects = GetEffects();
            return new KeyValuePair<InteractionType, List<Effect>>((InteractionType)EventTypeComboBox.SelectedItem, effects);
        }
        private KeyValuePair<TriggerType, List<Effect>> GetTriggerEffects()
        {
            if (trigInter != TrigOrInter.TRIGGER) throw new Exception("This is not an interaction control!");
            List<Effect> effects = GetEffects();
            return new KeyValuePair<TriggerType, List<Effect>>((TriggerType)EventTypeComboBox.SelectedItem, effects);
        }
    }
}
