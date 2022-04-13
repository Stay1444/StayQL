using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace StayQL.Managers
{
    class LanguageManager
    {
        public enum Language
        {
            Spanish = 0,
            English = 1,
        }

        private static readonly Dictionary<string, string[]> LDictionary = new Dictionary<string, string[]>()
        {
            { "window.connections.title", new string[]{ "Conexiones", "Connections" } },
            { "connections.connection.contextmenu.duplicateButton.header", new string[]{ "Duplicar", "Clone" } },
            { "connections.connection.contextmenu.deleteButton.header", new string[]{ "Eliminar", "Delete" } },
            { "window.ConnectionIntermediary.Title", new string[]{ "Conectandose a {0}", "Connecting to {0}" } },
            { "cancel", new string[]{ "Cancelar", "Cancel" } },
            { "close", new string[]{ "Cerrar", "Close" } },
            { "new", new string[]{ "Nueva", "New" } },
            { "newConection.defaultName", new string[]{ "Nueva Conexión", "New Connection" } },
            { "newConection.defaultDescription", new string[]{ "Una hermosa nueva conexion a un servidor sql.", "A new MySql connection." } },
            { "Error.DataGridError", new string[]{ "[ERROR] Parametro Invalido", "[ERROR] Invalid Parameter" } },
            { "NewSchemaWindow.default", new string[]{ "nuevo_schema", "new_schema" } },
            { "NewSchemaWindow.title", new string[]{ "Nuevo Schema", "New Schema" } },
            { "connect", new string[]{ "Conectar", "Connect" } },
            { "newconection.tooltip", new string[]{ "Nueva Conexión", "New Connection" } },
            { "newtablewindow.title", new string[]{ "Nueva Tabla", "New Table" } },
            { "newtablewindow.defaultTableName", new string[]{ "nueva_tabla", "new_table" } },
            { "notification.Connecting", new string[]{ "Connectandose a ", "Connecting to " } },
            { "notification.authFailed", new string[]{ "El usuario o la contraseña de {0} son incorrectos.", "Username or password for {0} are incorrect." } },
            { "notification.timedOut", new string[]{ "Tiempo de espera agotado al conectarse a {0}", "{0} Timed Out" } },
            { "confirm.deletion.title", new string[]{ "Cuidado!", "Warning!" } },
            { "confirm.deletion.description", new string[]{ "Vas a eliminar {0} lineas. ¿Estas seguro?", "You are going to delete {0} rows. Are you sure?" } },
            { "database.tree.newTable.header", new string[]{ "Nueva Tabla", "New Table" } },
            { "database.tree.deleteTable.header", new string[]{ "Eliminar Tabla", "Delete Table" } },
            { "newtable.column.name", new string[]{ "Nombre", "Name" } },
            { "newtable.column.type", new string[]{ "Tipo", "Type" } },
            { "newtable.column.length", new string[]{ "Tamaño", "Length" } },
            { "newtable.column.allowNull", new string[]{ "Permitir Null", "Allow Null" } },
            { "newtable.column.uniqueIndex", new string[]{ "Index Unico", "Unique Index" } },
            { "newtable.column.autoIncremental", new string[]{ "Auto Incremento", "Auto Incremental" } },
            { "newtable.column.DefaultValue", new string[]{ "Por defecto", "Default value" } },
            { "deleteTable.message.content", new string[]{ "Estas a punto de eliminar {0}. Estas seguro?\nEsta accion es irreversible.", "You are going to delete {0}. Are you sure?\nThis action cannot be reversed." } },
            { "deleteTable.message.title", new string[]{ "Eliminando {0}", "Deleting {0}" } },
            { "database.tree.modifyTable.header", new string[]{ "Editar", "Edit" } },
            { "File", new string[]{ "Archivo", "File" } },
            { "New Window", new string[]{ "Nueva Conexión", "New Connection" } },
            { "Settings", new string[]{ "Ajustes", "Settings" } },
            { "Show System Tables", new string[]{ "Mostrar tablas protegidas", "Show System Tables" } },
            { "Hide System Tables", new string[]{ "Ocultar tablas protegidas", "Hide System Tables" } },
            { "Language", new string[]{ "Idioma", "Language" } },
            { "Spanish", new string[]{ "Español", "Spanish" } },
            { "English", new string[]{ "Inglés", "English" } },
            { "Disconnect", new string[]{ "Desconectarse", "Disconnect" } },
            { "Open Connections File", new string[]{ "Abrir archivo de conexiones", "Open Connections File" } },
            { "Wipe Configuration", new string[]{ "Eliminar Configuracion", "Wipe Configuration" } },
            { "Exit", new string[]{ "Cerrar", "Exit" } },
            { "Export", new string[]{ "Exportar", "Export" } },
            { "Add Schema", new string[]{ "Añadir Schema", "Add Schema" } },
            { "New Query", new string[]{ "Nuevo Query", "New Query" } },
            { "Open Query File", new string[]{ "Abrir archivo Query", "Open Query File" } },
            { "delete", new string[]{ "Eliminar", "Delete" } },
            { "Server", new string[]{ "Servidor", "Server" } },
            { "New tab to current server", new string[]{ "Nueva ventana a este servidor" } },
            { "Status", new string[]{ "Estado" } },
            { "Client Connections", new string[]{ "Clientes Conectados", "Connected Clients" } },
        };


        public static Language SelectedLanguage = Language.English;

        public static string GetString(string key)
        {
            string result = key;
            if (LDictionary.ContainsKey(key))
            {

                string[] ph = LDictionary[key];

                if (ph.Length > (int)SelectedLanguage)
                {
                    result = ph[(int)SelectedLanguage];
                }

            }
            



            return result;
        }

    }
}
