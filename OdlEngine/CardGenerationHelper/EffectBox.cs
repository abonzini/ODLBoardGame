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
    /*
     * CHECKLIST FOR NEW ELEMENTS
     * Create label, box
     * Init the contents in constructor
     * Hide them in hider
     * Show them in the right event type
     */
    public partial class EffectBox : UserControl
    {
        TriginterEffects owner = null;
        public EffectBox()
        {
            InitializeComponent();
        }
        public void SetOwner(TriginterEffects owner)
        {
            // Sets ownership
            this.owner = owner;
        }
        private void EffectBox_Load(object sender, EventArgs e)
        {
            // Now preload each box
            TargetLocationBox.Items.AddRange(Enum.GetValues(typeof(TargetLocation)).Cast<object>().ToArray());
            TargetLocationBox.SelectedIndex = 0;
            TargetPlayerEnumBox.SetEnum(typeof(EntityOwner));
            SearchCriterionBox.Items.AddRange(Enum.GetValues(typeof(SearchCriterion)).Cast<object>().ToArray());
            SearchCriterionBox.SelectedIndex = 0;
            TargetTypeEnumBox.SetEnum(typeof(EntityType));
            ModifierOperationBox.Items.AddRange(Enum.GetValues(typeof(ModifierOperation)).Cast<object>().ToArray());
            ModifierOperationBox.SelectedIndex = 0;
            ModifierTargetBox.Items.AddRange(Enum.GetValues(typeof(ModifierTarget)).Cast<object>().ToArray());
            ModifierTargetBox.SelectedIndex = 0;
            InputRegisterBox.Items.AddRange(Enum.GetValues(typeof(Register)).Cast<object>().ToArray());
            InputRegisterBox.SelectedIndex = 0;
            OutputRegisterBox.Items.AddRange(Enum.GetValues(typeof(Register)).Cast<object>().ToArray());
            OutputRegisterBox.SelectedIndex = 0;
            // Finally, add the base one that will modify the rest
            EffectTypeComboBox.Items.AddRange(Enum.GetValues(typeof(EffectType)).Cast<object>().ToArray());
            EffectTypeComboBox.SelectedIndex = 0;
        }
        void ResetAll()
        {
            TargetLocationBox.SelectedIndex = 0;
            TargetLocationBox.Hide();
            TargetLocationLabel.Hide();
            TargetPlayerEnumBox.Clear();
            TargetPlayerEnumBox.Hide();
            TargetPlayerLabel.Hide();
            SearchCriterionBox.SelectedIndex = 0;
            SearchCriterionBox.Hide();
            SearchCriterionLabel.Hide();
            TargetTypeEnumBox.Clear();
            TargetTypeEnumBox.Hide();
            TargetTypeLabel.Hide();
            ModifierOperationBox.SelectedIndex = 0;
            ModifierOperationBox.Hide();
            ModifierOperationLabel.Hide();
            ModifierTargetBox.SelectedIndex = 0;
            ModifierTargetBox.Hide();
            ModifierTargetLabel.Hide();
            InputRegisterBox.SelectedIndex = 0;
            InputRegisterBox.Hide();
            InputRegisterLabel.Hide();
            OutputRegisterBox.SelectedIndex = 0;
            OutputRegisterBox.Hide();
            OutputRegisterLabel.Hide();
        }
        void ShowRelevant(EffectType effect)
        {
            Control[] relevant = effect switch
            {
                EffectType.TRIGGER_DEBUG => [],
                EffectType.DEBUG_STORE => [],
                EffectType.SELECT_ENTITY => [SearchCriterionBox, SearchCriterionLabel, TargetPlayerEnumBox, TargetPlayerLabel, TargetTypeEnumBox, TargetTypeLabel],
                EffectType.FIND_ENTITIES => [SearchCriterionBox, SearchCriterionLabel, TargetPlayerEnumBox, TargetPlayerLabel, TargetTypeEnumBox, TargetTypeLabel, TargetLocationBox, TargetLocationLabel, InputRegisterBox, InputRegisterLabel],
                EffectType.SUMMON_UNIT => [TargetPlayerLabel, TargetPlayerEnumBox, TargetLocationBox, TargetLocationLabel, InputRegisterBox, InputRegisterLabel],
                EffectType.MODIFIER => [ModifierOperationBox, ModifierOperationLabel, ModifierTargetBox, ModifierTargetLabel, InputRegisterBox, InputRegisterLabel, TargetPlayerEnumBox, TargetPlayerLabel, OutputRegisterBox, OutputRegisterLabel],
                _ => throw new NotImplementedException("Unhandled effect type"),
            };
            foreach (Control control in relevant)
            {
                control.Show();
            }
        }

        private void EffectTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EffectType type = (EffectType)EffectTypeComboBox.SelectedItem;
            ResetAll();
            ShowRelevant(type);
        }

        public Effect GetEffect()
        {
            return new Effect()
            {
                EffectType = (EffectType)EffectTypeComboBox.SelectedItem,
                TargetLocation = (TargetLocation)TargetLocationBox.SelectedItem,
                TargetPlayer = (EntityOwner)TargetPlayerEnumBox.GetEnumValue(),
                SearchCriterion = (SearchCriterion)SearchCriterionBox.SelectedItem,
                TargetType = (EntityType)TargetTypeEnumBox.GetEnumValue(),
                ModifierOperation = (ModifierOperation)ModifierOperationBox.SelectedItem,
                ModifierTarget = (ModifierTarget)ModifierTargetBox.SelectedItem,
                InputRegister = (Register)InputRegisterBox.SelectedItem,
                OutputRegister = (Register)OutputRegisterBox.SelectedItem,
                TempVariable = Convert.ToInt32(ValueUpDown.Value)
            };
        }
        // When I request to be deleted
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if(owner != null)
            {
                owner.RequestEffectDeletion(this);
            }
        }
    }
}
