
namespace UpdateInvTypes
{
    using System;
    using System.Windows.Forms;

    internal static class UpdateInvTypes
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UpdateInvTypesUI());
            }
            catch (Exception) { }
        }
    }
}