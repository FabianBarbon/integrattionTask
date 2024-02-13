using Microsoft.Extensions.Configuration;
using JupiterPlugin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterPlugin.Data
{
    internal class LayoutDbContext
    {
        private readonly string connectionString;

        public LayoutDbContext()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            string ipdb = configuration["jupiter:ip_db"];
            string userdb = configuration["jupiter:user_db"];
            string passdb = configuration["jupiter:user_db_password"];

            connectionString = "Data Source=DESKTOP-AHUUEGC\\SQLEXPRESS;Initial Catalog=Layouts;Integrated Security=False;User ID=Xtam;Password=Xtam2021*"; //$"Data Source=172.40.0.18;Initial Catalog=Layouts;User ID={userdb};Password={passdb};";
        }
        public ObservableCollection<Layout> GetEntities()
        {
            var entities = new ObservableCollection<Layout>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Layout where estado=1", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entities.Add(new Layout
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Row = Convert.ToInt32(reader["Row"]),
                                Columna = Convert.ToInt32(reader["Columna"]),
                                CreatedByUser = reader["CreatedByUser"].ToString(),
                                CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                                ModifiedByUser = (reader["ModifiedByUser"] == DBNull.Value) ? string.Empty : reader["ModifiedByUser"].ToString(),
                                ModificationDate = (reader["ModificationDate"] == DBNull.Value) ? DateTime.Now : Convert.ToDateTime(reader["ModificationDate"])
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return entities;
        }

        public void AddEntity(Layout entity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("INSERT INTO Layout (Name, Row, Columna, ObjectIsDefault, ObjectType, CreatedByUser, CreationDate) VALUES (@Name,@Row,@Columna,@ObjectIsDefault,@ObjectType,@CreatedByUser, @CreationDate)", connection))
                {
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@Row", entity.Row);
                    command.Parameters.AddWithValue("@Columna", entity.Columna);
                    command.Parameters.AddWithValue("@ObjectIsDefault", false);
                    command.Parameters.AddWithValue("@ObjectType", "layout");
                    command.Parameters.AddWithValue("@CreatedByUser", "admin");
                    command.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public void DeleteEntity(int entity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("UPDATE Layout SET estado=0 where ID=@ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", entity);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public ObservableCollection<MatrizModel> GetMatriz(long lngLayout)
        {
            var lstMatriz = new ObservableCollection<MatrizModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(string.Concat("SELECT * FROM Matriz where LayoutId=", lngLayout, " order by id asc"), connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lstMatriz.Add(new MatrizModel
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                IdLayout = Convert.ToInt32(reader["LayoutId"]),
                                Row = Convert.ToInt32(reader["Row"]),
                                Columna = Convert.ToInt32(reader["Columna"]),
                                Camera = (reader["CameraID"] == DBNull.Value) ? 0 : Convert.ToInt32(reader["CameraID"]),
                                Url = (reader["VideoUrl"] == DBNull.Value) ? string.Empty : reader["VideoUrl"].ToString(),
                                Guid_Camera = (reader["Guid_Camera"] == DBNull.Value) ? string.Empty : reader["Guid_Camera"].ToString()
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return lstMatriz;
        }
        public void UpdateMatriz(List<MatrizModel> lstMatriz)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                long lngLayout = lstMatriz[0].IdLayout;
                long lngRow = 0;
                long lngColumna = 0;

                using (SqlCommand command = new SqlCommand("DELETE Matriz where LayoutId=@lngLayout", connection))
                {
                    command.Parameters.AddWithValue("@lngLayout", lngLayout);
                    command.ExecuteNonQuery();
                }

                foreach (MatrizModel mat in lstMatriz)
                {
                    using (SqlCommand command = new SqlCommand("INSERT INTO Matriz (LayoutId, Row, Columna, CameraID, VideoUrl, Guid_Camera) VALUES (@IdLayout,@Row,@Columna,@Camera,@Url,@Guid_Camera)", connection))
                    {
                        command.Parameters.AddWithValue("@IdLayout", mat.IdLayout);
                        command.Parameters.AddWithValue("@Row", mat.Row);
                        command.Parameters.AddWithValue("@Columna", mat.Columna);
                        command.Parameters.AddWithValue("@Camera", mat.Camera);
                        command.Parameters.AddWithValue("@Url", mat.Url);
                        command.Parameters.AddWithValue("@Guid_camera", mat.Guid_Camera);
                        command.ExecuteNonQuery();
                    }
                    lngRow = mat.Row + 1;
                    lngColumna = mat.Columna + 1;
                }

                using (SqlCommand command = new SqlCommand("UPDATE Layout SET Row=@lngRow , Columna=@lngColumna where ID=@lngLayout", connection))
                {
                    command.Parameters.AddWithValue("@lngLayout", lngLayout);
                    command.Parameters.AddWithValue("@lngRow", lngRow);
                    command.Parameters.AddWithValue("@lngColumna", lngColumna);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        ///WALLS METHODS 
        ///
        //obetener muro
        public ObservableCollection<Wall> GetWalls()
        {

            var walls = new ObservableCollection<Wall>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Walls where state=1", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            walls.Add(new Wall
                            {

                                IdWall = Convert.ToInt32(reader["IdWall"]),
                                NameWall = reader["NameWall"].ToString(),
                                FK_CurrentLayout = Convert.ToInt32(reader["FK_CurrentLayout"]),
                                State = Convert.ToInt32(reader["State"]),
                                ScreenWidth = Convert.ToInt32(reader["ScreenWidth"]),
                                ScreenHeight = Convert.ToInt32(reader["ScreenHeight"]),
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return walls;
        }
        //add wall
        public void AddWall(Wall wall)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("INSERT INTO Walls (IdWall,NameWall,FK_CurrentLayaout,state) VALUES (@IdWall,@NameWall,@FK_CurrentLayout,@state)", connection))
                {
                    command.Parameters.AddWithValue("@IdWall", wall.IdWall);
                    command.Parameters.AddWithValue("@NameWall", wall.NameWall);
                    command.Parameters.AddWithValue("@FK_CurrentLayout", wall.FK_CurrentLayout);
                    command.Parameters.AddWithValue("@State", wall.State);
                    command.Parameters.AddWithValue("@ScreenWidth", wall.ScreenWidth);
                    command.Parameters.AddWithValue("@ScreenHeight", wall.ScreenHeight);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        //update wall
        public void UpdateWall(Wall wall)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("UPDATE Walls SET FK_CurrentLayout=@fk_CurrentLayout where NameWall=@NameWall", connection))
                {
                    command.Parameters.AddWithValue("@IdWall", wall.IdWall);
                    command.Parameters.AddWithValue("@NameWall", wall.NameWall);
                    command.Parameters.AddWithValue("@FK_CurrentLayout", wall.FK_CurrentLayout);
                    command.Parameters.AddWithValue("@State", wall.State);
                    command.Parameters.AddWithValue("@ScreenWidth", wall.ScreenWidth);
                    command.Parameters.AddWithValue("@ScreenHeight", wall.ScreenHeight);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        //eliminar muro
        public void DeleteWall(int entity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("UPDATE Walls SET estado=0 where ID=@ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", entity);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
