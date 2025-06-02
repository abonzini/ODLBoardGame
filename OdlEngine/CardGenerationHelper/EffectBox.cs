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
            InitializeEffectBox();
        }
        public void SetOwner(TriginterEffects owner)
        {
            // Sets ownership
            this.owner = owner;
        }
        void InitializeEffectBox()
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
            InputBox.Items.AddRange(Enum.GetValues(typeof(Variable)).Cast<object>().ToArray());
            InputBox.SelectedIndex = 0;
            OutputBox.Items.AddRange(Enum.GetValues(typeof(Variable)).Cast<object>().ToArray());
            OutputBox.SelectedIndex = 0;
            MultiVariableBox.Items.AddRange(Enum.GetValues(typeof(MultiInputProcessing)).Cast<object>().ToArray());
            MultiVariableBox.SelectedIndex = 0;
            // Finally, add the base one that will modify the rest
            EffectTypeComboBox.Items.AddRange(Enum.GetValues(typeof(EffectType)).Cast<object>().ToArray());
            EffectTypeComboBox.SelectedIndex = 0;
        }
        private void EffectBox_Load(object sender, EventArgs e)
        {
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
            InputBox.SelectedIndex = 0;
            InputBox.Hide();
            InputLabel.Hide();
            OutputBox.SelectedIndex = 0;
            OutputBox.Hide();
            OutputLabel.Hide();
            MultiVariableBox.SelectedIndex = 0;
            MultiVariableBox.Hide();
            MultiVariableLabel.Hide();
        }
        void ShowRelevant(EffectType effect)
        {
            Control[] relevant = effect switch
            {
                EffectType.TRIGGER_DEBUG => [],
                EffectType.DEBUG_STORE => [],
                EffectType.SELECT_ENTITY => [SearchCriterionBox, SearchCriterionLabel, TargetPlayerEnumBox, TargetPlayerLabel, TargetTypeEnumBox, TargetTypeLabel],
                EffectType.FIND_ENTITIES => [SearchCriterionBox, SearchCriterionLabel, TargetPlayerEnumBox, TargetPlayerLabel, TargetTypeEnumBox, TargetTypeLabel, TargetLocationBox, TargetLocationLabel, InputBox, InputLabel, MultiVariableBox, MultiVariableLabel],
                EffectType.SUMMON_UNIT => [TargetPlayerLabel, TargetPlayerEnumBox, TargetLocationBox, TargetLocationLabel, InputBox, InputBox, MultiVariableBox, MultiVariableLabel],
                EffectType.MODIFIER => [ModifierOperationBox, ModifierOperationLabel, InputBox, InputLabel, OutputBox, OutputLabel, TargetPlayerEnumBox, TargetPlayerLabel, MultiVariableBox, MultiVariableLabel],
                EffectType.ASSERT => [InputBox, InputLabel],
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
                Input = (Variable)InputBox.SelectedItem,
                Output = (Variable)OutputBox.SelectedItem,
                MultiInputProcessing = (MultiInputProcessing)MultiVariableBox.SelectedItem,
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
        public void SetEffect(Effect effect)
        {
            EffectTypeComboBox.SelectedItem = effect.EffectType;
            // Should be an auto update here
            TargetLocationBox.SelectedItem = effect.TargetLocation;
            TargetPlayerEnumBox.SetFlags((int)effect.TargetPlayer);
            SearchCriterionBox.SelectedItem = effect.SearchCriterion;
            TargetTypeEnumBox.SetFlags((int)effect.TargetType);
            ModifierOperationBox.SelectedItem = effect.ModifierOperation;
            InputBox.SelectedItem = effect.Input;
            OutputBox.SelectedItem = effect.Output;
            MultiVariableBox.SelectedItem = effect.MultiInputProcessing;
            ValueUpDown.Value = effect.TempVariable;
        }
    }
}
