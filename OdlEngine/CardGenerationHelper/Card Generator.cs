using ODLGameEngine;
using System.Drawing;
using System.Windows.Forms;

namespace CardGenerationHelper
{
    public partial class CardGenerator : Form
    {
        EntityBase currentEntity;
        EntityPlayInfo currentPlayInfo;
        EntityPrintInfo currentPrintInfo;

        EntityBase emptyEntity = new EntityBase();
        Unit unit = new Unit();
        Building building = new Building();
        PlayerState playerState = new PlayerState();
        Skill skill = new Skill();
        public CardGenerator()
        {
            InitializeComponent();
            currentEntity = emptyEntity;
            currentPlayInfo = emptyEntity.EntityPlayInfo;
            currentPrintInfo = emptyEntity.EntityPrintInfo;
            List<EntityBase> entities = [unit, building, playerState, skill];
            foreach (EntityBase entity in entities) // Set all to same info and fuck it
            {
                entity.EntityPlayInfo = currentPlayInfo;
                entity.EntityPrintInfo = currentPrintInfo;
            }

            DrawCard(); // Draw empty card
        }
        private void DrawCard()
        {
            int width = 2500; // 2.5x3.5 is resolution of a typical TCG card
            int height = 3500;

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Rectangle bounds = new Rectangle(0, 0, width, height);
                DrawHelper.DrawRoundedRectangle(g, bounds, 200, Color.Black, new SolidFillHelper() { FillColor = Color.LightGray }, 75);
            }

            CardPicture.Image = bitmap;
        }

        private void CardGenerator_Load(object sender, EventArgs e)
        {
            EntityTypeDropdown.Items.AddRange(Enum.GetValues(typeof(EntityType)).Cast<object>().ToArray());
            EntityTypeDropdown.SelectedIndex = 0;
            TargetOptionsDropdown.Items.AddRange(Enum.GetValues(typeof(TargetLocation)).Cast<object>().ToArray());
            TargetOptionsDropdown.SelectedIndex = 0;
            TargetConditionDropdown.Items.AddRange(Enum.GetValues(typeof(TargetCondition)).Cast<object>().ToArray());
            TargetConditionDropdown.SelectedIndex = 0;
            ExpansionDropdown.Items.AddRange(Enum.GetValues(typeof(ExpansionId)).Cast<object>().ToArray());
            ExpansionDropdown.SelectedIndex = 0;
            ClassDropdown.Items.AddRange(Enum.GetValues(typeof(PlayerClassType)).Cast<object>().ToArray());
            ClassDropdown.SelectedIndex = 0;
        }

        private void EntityTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            DrawCard();
        }

        private void TargetOptionsDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPlayInfo.TargetOptions = (TargetLocation)TargetOptionsDropdown.SelectedItem;
        }

        private void TargetConditionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPlayInfo.TargetConditions = (TargetCondition)TargetConditionDropdown.SelectedItem;
        }

        private void CardIdUpdown_ValueChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Id = Convert.ToInt32(CardIdUpdown.Value);
            DrawCard();
        }

        private void CardNameBox_TextChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Title = CardNameBox.Text.ToUpper();
            DrawCard();
        }

        private void ExpansionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Expansion = (ExpansionId)ExpansionDropdown.SelectedItem;
            DrawCard();
        }

        private void ClassDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPrintInfo.ClassType = (PlayerClassType)ClassDropdown.SelectedItem;
            DrawCard();
        }
    }
}
