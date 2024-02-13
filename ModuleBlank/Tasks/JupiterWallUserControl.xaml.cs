// ==========================================================================
// Copyright (C) 2016 by Genetec, Inc.
// All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.
// ==========================================================================
using Genetec.Sdk.Entities;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Media;
using Genetec.Sdk.Workspace;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Extensions.Configuration;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using JupiterPlugin.Data;
using JupiterPlugin.Models;
using JupiterPlugin.Helpers;
using Genetec.Sdk;
using static JupiterPlugin.Tasks.EntityTree;
using System.Diagnostics;



namespace JupiterPlugin.Tasks
{



        public partial class JupiterWallUserControl //
    {

        #region Constants

        public static int myUpDownControl;

        public static readonly DependencyProperty IsInPlaybackProperty =
                                DependencyProperty.Register("IsInPlayback", typeof(bool), typeof(JupiterWallUserControl), new PropertyMetadata(false));

        // Dependency property on the connection of the Sdk engine.
        public static readonly DependencyProperty IsSdkEngineConnectedProperty =
                                DependencyProperty.Register("IsSdkEngineConnected", typeof(bool), typeof(JupiterWallUserControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty OverlayNameProperty =
                                DependencyProperty.Register("OverlayName", typeof(string), typeof(JupiterWallUserControl), new PropertyMetadata("Show Status"));

        public static readonly DependencyProperty PlayPauseProperty =
                                DependencyProperty.Register("PlayPause", typeof(string), typeof(JupiterWallUserControl), new PropertyMetadata("Pause"));

        public static readonly DependencyProperty TimeStampProperty =
                                DependencyProperty.Register("TimeStamp", typeof(string), typeof(JupiterWallUserControl), new PropertyMetadata(""));

        public static readonly DependencyProperty TogglePlaybackProperty =
                                DependencyProperty.Register("TogglePlayback", typeof(string), typeof(JupiterWallUserControl), new PropertyMetadata("Switch to Playback"));
        public static readonly DependencyProperty IsAtLeastOneTilePlayingProperty =
                                DependencyProperty.Register("IsAtLeastOneTilePlaying", typeof(bool), typeof(JupiterWallUserControl), new PropertyMetadata(default(bool)));
        private readonly Engine m_sdkEngine = new Engine();

        

        private readonly List<Tile> m_tiles = new List<Tile>();

        //mine

        private readonly ObservableCollection<EntityItem> m_rootItems = new ObservableCollection<EntityItem>();

        #endregion

        #region Fields
        
           private readonly LayoutDbContext dbContext;
           private ObservableCollection<Layout> _entities;
           private ObservableCollection<Wall> _walls;



        //private Layout _entitie;                        

            private bool m_canDragDrop;
            private Tile m_selectedTile;
            private IConfigurationRoot configuration;

        #endregion
        


        #region Properties

        public bool IsInPlayback
        {
            get { return (bool)GetValue(IsInPlaybackProperty); }
            set { SetValue(IsInPlaybackProperty, value); }
        }

        public bool IsAtLeastOneTilePlaying
        {
            get { return (bool)GetValue(IsAtLeastOneTilePlayingProperty); }
            set { SetValue(IsAtLeastOneTilePlayingProperty, value); }
        }

        public bool IsSdkEngineConnected
        {
            get { return (bool)GetValue(IsSdkEngineConnectedProperty); }
            set { SetValue(IsSdkEngineConnectedProperty, value); }
        }

        public string OverlayName
        {
            get { return (string)GetValue(OverlayNameProperty); }
            set { SetValue(OverlayNameProperty, value); }
        }

        public string PlayPause
        {
            get { return (string)GetValue(PlayPauseProperty); }
            set { SetValue(PlayPauseProperty, value); }
        }

        public string TimeStamp
        {
            get { return (string)GetValue(TimeStampProperty); }
            set { SetValue(TimeStampProperty, value); }
        }

        public string TogglePlayback
        {
            get { return (string)GetValue(TogglePlaybackProperty); }
            set { SetValue(TogglePlaybackProperty, value); }
        }

        private IEngine SdkEngine
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public JupiterWallUserControl()
        {
            Debugger.Launch();
            InitializeComponent();           
            configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            //directorio genetec
            string ipGenetec = configuration["genetec:ip"];

            dbContext = new LayoutDbContext();
            DataContext = this;
            _entities = dbContext.GetEntities();
             
            tree.EntityFilter = new List<EntityType> { EntityType.Area, EntityType.Camera };
            tree.Initialize(m_sdkEngine);
          
            IsAtLeastOneTilePlaying = false;
            
            foreach (Tile tile in grid.Children.OfType<Tile>())
            {
                m_tiles.Add(tile);
                tile.player.HardwareAccelerationEnabled = true;
                tile.player.LivePlaybackModeToggled += OnPlayerLivePlaybackModeToggled;
                tile.player.FrameRendered += OnPlayerFrameRendered;
            }
              

            listBox.SelectionChanged += ListBox_SelectionChanged;
            muros.SelectionChanged += Muros_SelectionChanged;

            // Suscríbete al evento PropertyChanged
            FillListBox();
            FillSelect();
            //dpd.AddValueChanged(this, IsSdkEngineConnectedPropertyChanged);
            IsSdkEngineConnected = true;


          
        }

        #endregion

        #region Event Handlers
        //select walls chage

        private void Muros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("lita de muros");
            if (muros.SelectedItem != null)
            {
                Wall selectedWall = (Wall)muros.SelectedItem;
                int idWall = selectedWall.IdWall;
                string nameWall = selectedWall.NameWall;
                int FK_CurrentLayaout = selectedWall.FK_CurrentLayout;
                int state = selectedWall.State;

                // Buscar el elemento en _entities que tenga el mismo valor en la propiedad que estás utilizando (por ejemplo, "Name")
                var entityToSelect = _entities.FirstOrDefault(entity => entity.ID == FK_CurrentLayaout);

                // Seleccionar el elemento en el ListBox si se encuentra
                if (entityToSelect != null)
                {
                    listBox.SelectedItem = entityToSelect;
                }
            }
        }
       
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                Console.WriteLine("Cambio mi lista!@");
                if (listBox.SelectedItem != null)
                {
                    Layout selectedLayout = (Layout)listBox.SelectedItem;
                    Console.WriteLine($"ID: {selectedLayout.ID}, Name: {selectedLayout.Name}");
                    //MessageBox.Show($"ID: {selectedLayout.ID}, Name: {selectedLayout.Name}");
                    Nombre.Text = selectedLayout.Name;
                    Filas.Text = selectedLayout.Row.ToString();
                    IdLayaout.Text = selectedLayout.ID.ToString();
                    Columnas.Text = selectedLayout.Columna.ToString();

                    ObservableCollection<MatrizModel> lstMatriz = new ObservableCollection<MatrizModel>();
                    lstMatriz = dbContext.GetMatriz(selectedLayout.ID);

                    // Validar si lstMatriz está vacía
                    if (lstMatriz.Count == 0)
                    {
                        // Utilizar filas y columnas para crear la matriz de Tile
                        int numRows, numColumns;
                        if (int.TryParse(Filas.Text, out numRows) && int.TryParse(Columnas.Text, out numColumns))
                        {

                            //probemos layaout
                            // Llamar al método estático asincrónico desde la clase separada
                            //runLayaout();


                            // Crear una matriz ficticia de MatrizModel con valores predeterminados
                            for (int i = 0; i < numRows; i++)
                            {
                                for (int j = 0; j < numColumns; j++)
                                {
                                    lstMatriz.Add(new MatrizModel
                                    {
                                        ID = i * numColumns + j + 1,
                                        IdLayout = selectedLayout.ID,
                                        Row = i,
                                        Columna = j,
                                        Camera = 0,
                                        Url = $"URL_{i}_{j}"
                                    });
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ingresa valores válidos para filas y columnas.");
                            return;
                        }
                    }
                //Descomentarizar
                FillingGrid(lstMatriz); //lstMatriz

            }
        }
                
            
            private void OnEngineDirectoryCertificateValidation(object sender, DirectoryCertificateValidationEventArgs e)
            {
                MessageBoxResult result = MessageBox.Show("The identity of the Directory server cannot be verified. \n" +
                                "The certificate is not from a trusted certifying authority. \n" +
                                "Do you trust this server?", "Secure Communication", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    e.AcceptDirectory = true;
                }
            }

            private void OnEngineLoggedOff(object sender, LoggedOffEventArgs e)
            {
                IsSdkEngineConnected = m_sdkEngine.LoginManager.IsConnected;
            }

            private void OnEngineLogonFailed(object sender, LogonFailedEventArgs e)
            {
                MessageBox.Show(e.FormattedErrorMessage);
            }
     

            private void OnPlayerFrameRendered(object sender, FrameRenderedEventArgs e)
            {
                TimeStamp = e.FrameTime.ToLocalTime().ToString("HH:mm:ss.ffff");
            }

            private void OnPlayerLivePlaybackModeToggled(object sender, EventArgs e)
            {
                IsInPlayback = !(PlayersPlayingLive() || GetFirstPopulatedTile() == null);
            }

            private void OnTileDragOver(object sender, DragEventArgs e)
            {
                e.Effects = DragDropEffects.None;
                if (e.Data.GetDataPresent(typeof(Guid)))
                {
                    Entity entity = m_sdkEngine.GetEntity((Guid)e.Data.GetData(typeof(Guid)));
                    if ((entity != null) && (entity.EntityType == EntityType.Camera))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                }
                
            }

            // NOTE : This sample is meant to demonstrate the possibility to use the WPF drag and drop feature as well as multiple _tiles to show video feeds.
            // It isn't meant to be a guide on how to use the WPF drag and drop. The implementation may therefore not be bulletproof.
            private void OnTileDrop(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent(typeof(Guid)))
                {
                    Entity entity = m_sdkEngine.GetEntity((Guid)e.Data.GetData(typeof(Guid)));
                    if ((entity != null) && (entity.EntityType == EntityType.Camera))
                    {
                        Tile tile = sender as Tile;
                        //Take the playing state of the cameras before adding one
                        bool wasLive = PlayersPlayingLive();
                        if (tile != null)
                        {
                            tile.StopTile();
                            tile.InitializeTile(entity, m_sdkEngine);
                            //If all _tiles are live or if we're adding the first camera
                            if (wasLive || GetFirstPopulatedTile() == null)
                            {
                                tile.player.PlayLive();
                            }
                            else
                            {
                                tile.player.AddToSynchronizedPlayback();
                            }
                        }
                    }
                }
               
                m_canDragDrop = false;
            }

            private void OnTileMouseDown(object sender, MouseButtonEventArgs e)
            {
                if (m_selectedTile != null)
                {
                    m_selectedTile.BorderBrush = Brushes.Black;
                }
                m_selectedTile = sender as Tile;
                if (m_selectedTile != null)
                {
                    m_selectedTile.BorderBrush = Brushes.Red;
                    m_canDragDrop = true;
                }
                
            }

            private void OnTileMouseMove(object sender, MouseEventArgs e)
            {
                Tile tile = sender as Tile;
                if (tile != null && e.LeftButton == MouseButtonState.Pressed && m_canDragDrop)
                {
                    Guid tileCopyGuid = tile.m_playerGuid;
                    tile.StopTile();
                    if (DragDrop.DoDragDrop(this, tileCopyGuid, DragDropEffects.Copy) == DragDropEffects.None)
                    {
                        m_canDragDrop = false;
                    }
                }
               
            }

            private void OnWindowKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Back)
                {
                    m_selectedTile.StopTile();
                }
               
            }
 
            private void OnWindowMouseUp(object sender, MouseEventArgs e)
            {
                m_canDragDrop = false;
                tree.m_canDragDrop = false;
            }

            #endregion

            #region Private Methods

            private Tile GetFirstPopulatedTile()
            {
                return m_tiles.FirstOrDefault(tile => tile.IsStarted());
            }

            private bool PlayersPlayingLive()
            {
                if (m_tiles.Count(tile => tile.IsStarted()) == 0)
                {
                    return false;
                }
                //Returns true if all initialized players are live
                return m_tiles.Where(tile => tile.IsStarted()).All(tile => tile.player.IsPlayingLiveStream);
            }
      
        

        private void AgregarTile_Click(object sender, RoutedEventArgs e)
        {
            // Crea un nuevo Tile
            Tile nuevoTile = new Tile
            {
                Name = "tile" + grid.Children.Count, // Asigna un nombre único al Tile
                Row = grid.Children.Count + 1, // Puedes ajustar esto según tus necesidades
                Column = 0, // Puedes ajustar esto según tus necesidades
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Margin = new Thickness(-2, 10, 2, 185),
            };

            // Asocia eventos
            nuevoTile.MouseDown += OnTileMouseDown;
            nuevoTile.MouseMove += OnTileMouseMove;
            nuevoTile.Drop += OnTileDrop;
            nuevoTile.AllowDrop = true;
            nuevoTile.DragOver += OnTileDragOver;



            // Agrega el nuevo Tile al Grid
            grid.Children.Add(nuevoTile);

        }
        



        private void OnTileMouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                if (sender is Tile clickedTile)
                {
                    // Obtén la posición de la Tile en la matriz
                    int fila = Grid.GetRow(clickedTile);
                    int columna = Grid.GetColumn(clickedTile);

                    // Obtén el BitRate y CameraName de esa posición
                    string bitRate = clickedTile.BitRate;
                    string cameraName = clickedTile.CameraName;

                    // Imprime los valores
                    MessageBox.Show($"BitRate: {bitRate}, CameraName: {cameraName}");
                }
            }

            private string ObtenerBitRate(int fila, int columna)
            {
                // Lógica para obtener el BitRate según la posición
                // Puedes ajustar esto según tus necesidades específicas
                return $"bitrate_{fila}_{columna}";
            }
            private string ObtenerCameraName(int fila, int columna)
            {
                // Lógica para obtener el CameraName según la posición
                // Puedes ajustar esto según tus necesidades específicas
                return $"camera_name_{fila}_{columna}";
            }

        #endregion

        #region Fill lists
        
        private void FillListBox()
        {
            _entities = dbContext.GetEntities();
            listBox.ItemsSource = _entities;
            listBox.DisplayMemberPath = "Name";

        }

        //llenar muroas
        
        private void FillSelect()
        {
            _walls = dbContext.GetWalls();
            muros.ItemsSource = _walls;
            muros.DisplayMemberPath = "NameWall";

            // Seleccionar el primer elemento
            if (_walls.Count > 0)
            {
                muros.SelectedIndex = 0;

                var selectedLayout = _walls[0].FK_CurrentLayout;
                // Buscar el elemento en _entities que tenga el mismo valor en la propiedad que estás utilizando (por ejemplo, "Name")
                var entityToSelect = _entities.FirstOrDefault(entity => entity.ID == selectedLayout);

                // Seleccionar el elemento en el ListBox si se encuentra
                if (entityToSelect != null)
                {
                    listBox.SelectedItem = entityToSelect;
                }
            }
        }
 

        #endregion

        #region Events Jupiter
        //logica api y vista jupyter

        /////////////////////////////////////////////////
      
        private void FillingGrid(ObservableCollection<MatrizModel> lstCaragrMatriz)
        {
            if (int.TryParse(Filas.Text, out int numFilas) && int.TryParse(Columnas.Text, out int numColumnas))
            {
                Grid nuevoGrid = new Grid();
                for (int i = 0; i < numFilas; i++)
                {
                    nuevoGrid.RowDefinitions.Add(new RowDefinition());
                }

                for (int j = 0; j < numColumnas; j++)
                {
                    nuevoGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                LayoutGrid.Children.Clear();
                LayoutGrid.Children.Add(nuevoGrid);

                foreach (var item in lstCaragrMatriz)
                {
                    // Convertir la posición de la matriz a fila y columna
                    var i = item.Row;
                    var j = item.Columna;

                    // Crear etiqueta para mostrar el contenido
                    Label contentLabel = new Label
                    {
                        Content = item.Url, // Puedes ajustar esto para mostrar otras propiedades
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, -20, 0, 0) // Ajusta el margen para colocar la etiqueta encima del Tile
                    };

                    // Crear Tile
                    Tile tile = new Tile
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(44, 52, 60)),
                        BorderThickness = new Thickness(2),
                        AllowDrop = true,
                        BitRate = ObtenerBitRate(i, j),
                        CameraName = item.Url,//ObtenerCameraName(i, j),
                        Tag = item.ID // Asignar el ID como Tag para identificación única
                    };


                    // Establecer el color de fondo del Tile
                    tile.Background = new SolidColorBrush(Color.FromRgb(153, 153, 153)); // #007fff
                                                                                         // Agrega eventos después de crear la instancia de Tile
                    tile.MouseDown += OnTileMouseDown;
                    tile.MouseMove += OnTileMouseMove;
                    tile.Drop += OnTileDrop;
                    tile.DragOver += OnTileDragOver;
                    tile.MouseDoubleClick += OnTileMouseDoubleClick;
                    if (!string.IsNullOrEmpty(item.Guid_Camera) && item.Guid_Camera != "00000000-0000-0000-0000-000000000000" && IsSdkEngineConnected)
                    {
                        Guid guidCamera = new Guid(item.Guid_Camera);
                        tile.m_playerGuid = guidCamera;
                        Entity entity = m_sdkEngine.GetEntity(guidCamera);
                        tile.StopTile();
                        tile.InitializeTile(entity, m_sdkEngine);
                        //If all _tiles are live or if we're adding the first camera
                        if (GetFirstPopulatedTile() == null)
                        {
                            tile.player.PlayLive();
                        }
                        else
                        {
                            tile.player.AddToSynchronizedPlayback();
                        }

                    }

                    // Establece la posición en el Grid
                    Grid.SetRow(tile, i);
                    Grid.SetColumn(tile, j);
                    Grid.SetRow(contentLabel, i);
                    Grid.SetColumn(contentLabel, j);

                    // Agregar el Tile como hijo del Grid
                    nuevoGrid.Children.Add(tile);
                    nuevoGrid.Children.Add(contentLabel);

                }
            }
            else
            {
                MessageBox.Show("Ingresa valores válidos para filas y columnas.");
            }
        }
        
        
        private void Add_Layout_Template_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Nombre.Text))
                {
                    if (int.TryParse(Filas.Text, out int numFilas) && int.TryParse(Columnas.Text, out int numColumnas))
                    {

                        if (numFilas > 8 && numColumnas > 8)
                        {
                            MessageBox.Show("La matriz esta restringida a un tamano maximo de 8x8");
                        }
                        else
                        {
                            Layout layoutObj = new Layout
                            {
                                ID = 2,
                                Name = Nombre.Text,
                                Row = numFilas,
                                Columna = numColumnas,
                                ObjectIsDefault = true,
                                ObjectType = "SampleType",
                                AspectTypes = "SampleAspect",
                                CreatedByUser = "SampleUser",
                                CreationDate = DateTime.Now,
                                ModifiedByUser = "ModifiedUser",
                                ModificationDate = DateTime.Now,
                                estado = true
                            };
                            dbContext.AddEntity(layoutObj);
                            FillListBox();
                            listBox.SelectedItem = _entities.FirstOrDefault(entity => entity.Name == layoutObj.Name);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Valores de filas y columnas invalido", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }
                else
                {
                    MessageBox.Show("Ingrese un nombre de layout", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Error al intentar agregar un nuevo template.");
            }

        }

        
        private void Remove_layout_template_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Layout selectedLayout = (Layout)listBox.SelectedItem;
                dbContext.DeleteEntity(selectedLayout.ID);
                FillListBox();
                MessageBox.Show("Entidad eliminada correctamente.");
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
                MessageBox.Show("Entidad No se pudo eliminar.");

            }



        }
        
        

        private void Save_Layout_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                SaveLayoutDb();

            }
            catch (Exception)
            {

                MessageBox.Show("Error al guardar el layout");
            }

        }
                

        
        private void Apply_Layout_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                if (string.IsNullOrEmpty(Nombre.Text))
                {
                    MessageBox.Show("Asigne un nombre al Layout");
                    return;
                }
                Wall selectedWall = (Wall)muros.SelectedItem;

                // Mostrar el cuadro de diálogo de advertencia con opciones de sí/no
                string messajeWarning = $"¿Esta seguro de aplicar este cambio en el Wall {selectedWall.NameWall} ?";
                MessageBoxResult result = MessageBox.Show(messajeWarning, "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Procesar la respuesta del usuario
                if (result != MessageBoxResult.Yes)
                {
                    //MessageBox.Show("Operación cancelada. Saliendo...");
                    return;
                }

                List<MatrizModel> lstMatriz = new List<MatrizModel>();
                if (LayoutGrid.Children.Count > 0 && LayoutGrid.Children[0] is Grid nuevoGrid)
                {
                    int numFilas = nuevoGrid.RowDefinitions.Count;
                    int numColumnas = nuevoGrid.ColumnDefinitions.Count;

                    for (int i = 0; i < numFilas; i++)
                    {
                        for (int j = 0; j < numColumnas; j++)
                        {
                            Tile tile = nuevoGrid.Children
                                .OfType<Tile>()
                                .FirstOrDefault(t => Grid.GetRow(t) == i && Grid.GetColumn(t) == j);

                            if (tile != null)
                            {

                                Console.WriteLine($"Tile en posición [{i}, {j}]: BitRate = {tile.BitRate}, CameraName = {tile.CameraName}");
                                lstMatriz.Add(new MatrizModel
                                {

                                    //Name = tile.Name,
                                    IdLayout = int.Parse(IdLayaout.Text),
                                    Row = i,
                                    Columna = j,
                                    Camera = 0,
                                    Url = tile.CameraName,
                                    Guid_Camera = tile.m_playerGuid.ToString()
                                });
                            }
                        }
                    }
                    selectedWall.FK_CurrentLayout = int.Parse(IdLayaout.Text);
                    dbContext.UpdateMatriz(lstMatriz);
                    dbContext.UpdateWall(selectedWall);
                    RunLayaout(selectedWall.ScreenWidth, selectedWall.ScreenHeight, numFilas, numColumnas, lstMatriz);
                    //aqui va logica para armar JSON con la posicion de las camaras 

                    ///
                    MessageBox.Show("Layaout guardado  y aplicado correctamente!");
                }
                else
                {
                    MessageBox.Show("La matriz no ha sido creada o está vacía.");
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Error al intentar Aplicar el layout.");
            }

        }
        

        
        private void SaveLayoutDb()
        {
            try
            {
                if (string.IsNullOrEmpty(Nombre.Text))
                {
                    MessageBox.Show("Asigne un nombre al Layout");
                    return;
                }
                Wall selectedWall = (Wall)muros.SelectedItem;

                // Mostrar el cuadro de diálogo de advertencia con opciones de sí/no
                string messajeWarning = $"¿Esta seguro de guardar este cambio en el Wall {selectedWall.NameWall} ?";
                MessageBoxResult result = MessageBox.Show(messajeWarning, "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Procesar la respuesta del usuario
                if (result != MessageBoxResult.Yes)
                {
                    //MessageBox.Show("Operación cancelada. Saliendo...");
                    return;
                }

                List<MatrizModel> lstMatriz = new List<MatrizModel>();
                if (LayoutGrid.Children.Count > 0 && LayoutGrid.Children[0] is Grid nuevoGrid)
                {
                    int numFilas = nuevoGrid.RowDefinitions.Count;
                    int numColumnas = nuevoGrid.ColumnDefinitions.Count;

                    for (int i = 0; i < numFilas; i++)
                    {
                        for (int j = 0; j < numColumnas; j++)
                        {
                            Tile tile = nuevoGrid.Children
                                .OfType<Tile>()
                                .FirstOrDefault(t => Grid.GetRow(t) == i && Grid.GetColumn(t) == j);

                            if (tile != null)
                            {

                                Console.WriteLine($"Tile en posición [{i}, {j}]: BitRate = {tile.BitRate}, CameraName = {tile.CameraName}");
                                lstMatriz.Add(new MatrizModel
                                {

                                    //Name = tile.Name,
                                    IdLayout = int.Parse(IdLayaout.Text),
                                    Row = i,
                                    Columna = j,
                                    Camera = 0,
                                    Url = tile.CameraName,
                                    Guid_Camera = tile.m_playerGuid.ToString()
                                });
                            }
                        }
                    }
                    selectedWall.FK_CurrentLayout = int.Parse(IdLayaout.Text);
                    dbContext.UpdateMatriz(lstMatriz);
                    dbContext.UpdateWall(selectedWall);

                    MessageBox.Show("Layaout guardado y aplicado correctamente!");
                }
                else
                {
                    MessageBox.Show("La matriz no ha sido creada o está vacía.");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        
                private static object ScreenPosition(int screenWidth, int screenHeight, int rows, int cols, List<MatrizModel> lstMatriz)
        {
            // Crear un objeto anónimo para almacenar la información de cada cuadro
            var matrixInfo = new
            {
                ScreenWidth = screenWidth,
                ScreenHeight = screenHeight,
                Boxes = new List<object>()
            };

            try
            {
                // Calcular la altura y anchura de cada cuadro
                int boxWidth = screenWidth / cols;
                int boxHeight = screenHeight / rows;



                // Iterar sobre la matriz y agregar la información de cada cuadro al objeto
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int x = col * boxWidth;
                        int y = row * boxHeight;

                        var box = new
                        {
                            X = x,
                            Y = y,
                            Width = boxWidth,
                            Height = boxHeight
                        };

                        matrixInfo.Boxes.Add(box);
                    }
                }

                // Convertir el objeto a formato JSON y retornar el resultado
                return matrixInfo;

            }
            catch (Exception)
            {

                MessageBox.Show("Error al intentar generar la posicion de la camara.");
                return matrixInfo;
            }
        }

        /// <summary>
        ///RunLayaout
        /// </summary>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="lstMatriz"></param>
        /// 


        public async void RunLayaout(int screenWidth, int screenHeight, int rows, int cols, List<MatrizModel> lstMatriz)
        {
            try
            {
                Console.WriteLine("Estoy en ! runLayaout");
                // Calcular la altura y anchura de cada cuadro
                int boxWidth = screenWidth / cols;
                int boxHeight = screenHeight / rows;

                // Crear un objeto anónimo para almacenar la información de cada cuadro
                var matrixInfo = new
                {
                    ScreenWidth = screenWidth,
                    ScreenHeight = screenHeight,
                    Boxes = new List<object>()
                };

                // Iterar sobre la matriz y agregar la información de cada cuadro al objeto
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        int x = col * boxWidth;
                        int y = row * boxHeight;


                        var box = new
                        {
                            X = x,
                            Y = y,
                            Width = boxWidth,
                            Height = boxHeight
                        };
                        matrixInfo.Boxes.Add(box);
                    }
                }
                //get name wall
                Wall selectedWall = (Wall)muros.SelectedItem;
                string nameWall = selectedWall.NameWall;
                //get name layout
                string nameLayout = Nombre.Text;
                int windowsID = 30000;

                //armar mi request 

                var requestData = new
                {
                    wall = nameWall,
                    layout = nameLayout,
                    windowsIn = new List<object>()

                };

                for (int i = 0; i < lstMatriz.Count; i++)
                {
                    dynamic box = matrixInfo.Boxes[i];

                    // Acceder a las propiedades X e Y del i-ésimo elemento en matrixInfo.Boxes
                    int x = box.X;
                    int y = box.Y;
                    int widthBox = box.Width;
                    int heightBox = box.Height;
                    windowsID = windowsID + 1;
                    string userGenetec = configuration["genetec:user_gateway"];
                    string passGenetec = configuration["genetec:user_gateway_password"];
                    string ipGenetec = configuration["genetec:ip"];
                    string puertoGenetec = configuration["genetec:puerto_gateway"];
                    var wind = new
                    {
                        idWindow = windowsID,
                        source = lstMatriz[i].Url,
                        urlRTSP = $"rtsp://{userGenetec}:{passGenetec}@{ipGenetec}:{puertoGenetec}/{lstMatriz[i].Guid_Camera}/live",
                        xaxix = x,
                        yaxix = y,
                        width = widthBox,
                        height = heightBox,
                        ZOrder = 1
                    };

                    requestData.windowsIn.Add(wind);
                }
                //envio de peticion a la API
                string api_host = configuration["jupiter:API_HOST"];
                bool result = await RestClientHelper.CreateDynamicLayout(requestData, api_host);

                // Hacer algo con el resultado si es necesario
                if (result)
                {
                    Console.WriteLine("se llamo al layout con exito! " + result);
                }
                else
                {
                    MessageBox.Show("Error al intentar enviar la peticion a la PDC.");
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Error al intentar enviar la peticion a la PDC.");
            }

        }

        ///////////////////////////////////////


        #endregion

       
    }
}