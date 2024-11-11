using NAPLPS;

namespace NAPLPSApp.Forms;

public partial class NewCommandForm : Form
{
    public NewCommandForm()
    {
        InitializeComponent();
    }

    private void NewCommandForm_Load(object sender, EventArgs e)
    {
        var commands = NaplpsUtils.GetAddCommands();

        // add to combobox
        foreach (var command in commands)
        {
            comboBoxAddCommands.Items.Add(command);
        }
    }
}
