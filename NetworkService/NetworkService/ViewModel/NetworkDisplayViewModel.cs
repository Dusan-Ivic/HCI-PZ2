using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
	public class NetworkDisplayViewModel : BindableBase
	{
		public BindingList<Entity> EntitiesInList { get; set; }

		public ObservableCollection<Canvas> CanvasCollection { get; set; }
		public ObservableCollection<MyLine> LineCollection { get; set; }
		public ObservableCollection<Brush> BorderBrushCollection { get; set; }

		private Entity selectedEntity;

		private Entity draggedItem = null;
		private bool dragging = false;
		public int draggingSourceIndex = -1;

		public MyICommand<object> DropEntityOnCanvas { get; set; }
		public MyICommand<object> LeftMouseButtonDownOnCanvas { get; set; }
		public MyICommand MouseLeftButtonUp { get; set; }
		public MyICommand<object> SelectionChanged { get; set; }
		public MyICommand<object> OslobodiCanvas { get; set; }
		public MyICommand<object> RightMouseButtonDownOnCanvas { get; set; }

		private bool isLineSourceSelected = false;
		private int sourceCanvasIndex = -1;
		private int destinationCanvasIndex = -1;
		private MyLine currentLine = new MyLine();
		private Point linePoint1 = new Point();
		private Point linePoint2 = new Point();

		public NetworkDisplayViewModel()
		{
			EntitiesInList = new BindingList<Entity>();

			CanvasCollection = new ObservableCollection<Canvas>();
			for (int i = 0; i < 12; i++)
			{
				CanvasCollection.Add(new Canvas()
				{
					Background = Brushes.LightGray,
					AllowDrop = true
				});
			}

			BorderBrushCollection = new ObservableCollection<Brush>();
			for (int i = 0; i < 12; i++)
			{
				BorderBrushCollection.Add(Brushes.DarkGray);
			}

			LineCollection = new ObservableCollection<MyLine>();

			DropEntityOnCanvas = new MyICommand<object>(OnDrop);
			LeftMouseButtonDownOnCanvas = new MyICommand<object>(OnLeftMouseButtonDown);
			MouseLeftButtonUp = new MyICommand(OnMouseLeftButtonUp);
			SelectionChanged = new MyICommand<object>(OnSelectionChanged);
			OslobodiCanvas = new MyICommand<object>(OnOslobodiCanvas);
			RightMouseButtonDownOnCanvas = new MyICommand<object>(OnRightMouseButtonDown);
		}

		public Entity SelectedEntity
		{
			get { return selectedEntity; }
			set
			{
				selectedEntity = value;
				OnPropertyChanged("SelectedEntity");
			}
		}

		private void OnDrop(object parameter)
		{
			if (draggedItem != null)
			{
				int index = Convert.ToInt32(parameter);

				if (CanvasCollection[index].Resources["taken"] == null)
				{
					BitmapImage logo = new BitmapImage();
					logo.BeginInit();
					logo.UriSource = new Uri(draggedItem.Type.ImgSrc, UriKind.RelativeOrAbsolute);
					logo.EndInit();

					CanvasCollection[index].Background = new ImageBrush(logo);
					CanvasCollection[index].Resources.Add("taken", true);
					CanvasCollection[index].Resources.Add("data", draggedItem);
					BorderBrushCollection[index] = (draggedItem.IsValueValidForType()) ? Brushes.Green : Brushes.Red;

					// PREVLACENJE IZ DRUGOG CANVASA
					if (draggingSourceIndex != -1)
					{
						CanvasCollection[draggingSourceIndex].Background = Brushes.LightGray;
						CanvasCollection[draggingSourceIndex].Resources.Remove("taken");
						CanvasCollection[draggingSourceIndex].Resources.Remove("data");
						BorderBrushCollection[draggingSourceIndex] = Brushes.DarkGray;

						// Crtanje linije se prekida ako je entitet pomeren na drugu canvas kontrolu izmedju postavljanja tacaka
						if (sourceCanvasIndex != -1)
						{
							isLineSourceSelected = false;
							sourceCanvasIndex = -1;
							linePoint1 = new Point();
							linePoint2 = new Point();
							currentLine = new MyLine();
						}

						UpdateLinesForCanvas(draggingSourceIndex, index);

						draggingSourceIndex = -1;
					}

					// PREVLACENJE IZ LISTE
					if (EntitiesInList.Contains(draggedItem))
					{
						EntitiesInList.Remove(draggedItem);
					}
				}
			}
		}

		private void OnLeftMouseButtonDown(object parameter)
		{
			if (!dragging)
			{
				int index = Convert.ToInt32(parameter);

				if (CanvasCollection[index].Resources["taken"] != null)
				{
					dragging = true;
					draggedItem = (Entity)(CanvasCollection[index].Resources["data"]);
					draggingSourceIndex = index;
					DragDrop.DoDragDrop(CanvasCollection[index], draggedItem, DragDropEffects.Move);
				}
			}
		}

		private void OnMouseLeftButtonUp()
		{
			draggedItem = null;
			SelectedEntity = null;
			dragging = false;
			draggingSourceIndex = -1;
		}

		private void OnSelectionChanged(object parameter)
		{
			if (!dragging)
			{
				dragging = true;
				draggedItem = SelectedEntity;
				DragDrop.DoDragDrop((ListView)parameter, draggedItem, DragDropEffects.Move);
			}
		}

		private void OnOslobodiCanvas(object parameter)
		{
			int index = Convert.ToInt32(parameter);

			if (CanvasCollection[index].Resources["taken"] != null)
			{
				DeleteLinesForCanvas(index);

				EntitiesInList.Add((Entity)CanvasCollection[index].Resources["data"]);
				CanvasCollection[index].Background = Brushes.LightGray;
				CanvasCollection[index].Resources.Remove("taken");
				CanvasCollection[index].Resources.Remove("data");
				BorderBrushCollection[index] = Brushes.DarkGray;
			}
		}

		private void OnRightMouseButtonDown(object parameter)
		{
			int index = Convert.ToInt32(parameter);

			if (CanvasCollection[index].Resources["taken"] != null)
			{
				if (!isLineSourceSelected)
				{
					sourceCanvasIndex = index;

					linePoint1 = GetPointForCanvasIndex(sourceCanvasIndex);

					currentLine.X1 = linePoint1.X;
					currentLine.Y1 = linePoint1.Y;
					currentLine.Source = sourceCanvasIndex;

					isLineSourceSelected = true;
				}
				else
				{
					destinationCanvasIndex = index;

					if ((sourceCanvasIndex != destinationCanvasIndex) && !DoesLineAlreadyExist(sourceCanvasIndex, destinationCanvasIndex))
					{
						linePoint2 = GetPointForCanvasIndex(destinationCanvasIndex);

						currentLine.X2 = linePoint2.X;
						currentLine.Y2 = linePoint2.Y;
						currentLine.Destination = destinationCanvasIndex;

						LineCollection.Add(new MyLine
						{
							X1 = currentLine.X1,
							Y1 = currentLine.Y1,
							X2 = currentLine.X2,
							Y2 = currentLine.Y2,
							Source = currentLine.Source,
							Destination = currentLine.Destination
						});

						isLineSourceSelected = false;

						linePoint1 = new Point();
						linePoint2 = new Point();
						currentLine = new MyLine();
					}
					else
					{
						// Pocetak i kraj linije su u istom canvasu

						isLineSourceSelected = false;

						linePoint1 = new Point();
						linePoint2 = new Point();
						currentLine = new MyLine();
					}
				}
			}
			else
			{
				// Canvas na koji se postavlja tacka nije zauzet

				isLineSourceSelected = false;

				linePoint1 = new Point();
				linePoint2 = new Point();
				currentLine = new MyLine();
			}
		}

		// Linije koje su povezane sa canvasom na indexu SOURCE povezati sa canvasom na indexu DESTINATION
		private void UpdateLinesForCanvas(int sourceCanvas, int destinationCanvas)
		{
			for (int i = 0; i < LineCollection.Count; i++)
			{
				if (LineCollection[i].Source == sourceCanvas)
				{
					Point newSourcePoint = GetPointForCanvasIndex(destinationCanvas);
					LineCollection[i].X1 = newSourcePoint.X;
					LineCollection[i].Y1 = newSourcePoint.Y;
					LineCollection[i].Source = destinationCanvas;
				}
				else if (LineCollection[i].Destination == sourceCanvas)
				{
					Point newDestinationPoint = GetPointForCanvasIndex(destinationCanvas);
					LineCollection[i].X2 = newDestinationPoint.X;
					LineCollection[i].Y2 = newDestinationPoint.Y;
					LineCollection[i].Destination = destinationCanvas;
				}
			}
		}

		// Da li je canvas povezan linijama
		private bool IsCanvasConnected(int canvasIndex)
		{
			foreach (MyLine line in LineCollection)
			{
				if ((line.Source == canvasIndex) || (line.Destination == canvasIndex))
				{
					return true;
				}
			}
			return false;
		}

		// Da li vec postoji linija izmedju dva canvasa
		private bool DoesLineAlreadyExist(int source, int destination)
		{
			foreach (MyLine line in LineCollection)
			{
				if ((line.Source == source) && (line.Destination == destination))
				{
					return true;
				}
				if ((line.Source == destination) && (line.Destination == source))
				{
					return true;
				}
			}
			return false;
		}

		public void DeleteEntityFromCanvas(Entity entity)
		{
			int canvasIndex = GetCanvasIndexForEntityId(entity.Id);

			if (canvasIndex != -1)
			{
				CanvasCollection[canvasIndex].Background = Brushes.LightGray;
				CanvasCollection[canvasIndex].Resources.Remove("taken");
				CanvasCollection[canvasIndex].Resources.Remove("data");
				BorderBrushCollection[canvasIndex] = Brushes.DarkGray;

				DeleteLinesForCanvas(canvasIndex);
			}
		}

		private void DeleteLinesForCanvas(int canvasIndex)
		{
			List<MyLine> linesToDelete = new List<MyLine>();

			for (int i = 0; i < LineCollection.Count; i++)
			{
				if ((LineCollection[i].Source == canvasIndex) || (LineCollection[i].Destination == canvasIndex))
				{
					linesToDelete.Add(LineCollection[i]);
				}
			}

			foreach (MyLine line in linesToDelete)
			{
				LineCollection.Remove(line);
			}
		}

		// Centralna tacka na canvasu
		private Point GetPointForCanvasIndex(int canvasIndex)
		{
			double x = 0, y = 0;

			for (int row = 0; row <= 3; row++)
			{
				for (int col = 0; col <= 2; col++)
				{
					int currentIndex = row * 3 + col;

					if (canvasIndex == currentIndex)
					{
						x = 44 + (col * 88);
						y = 44 + (row * 88);

						break;
					}
				}
			}
			return new Point(x, y);
		}

		public int GetCanvasIndexForEntityId(int entityId)
		{
			for (int i = 0; i < CanvasCollection.Count; i++)
			{
				Entity entity = (CanvasCollection[i].Resources["data"]) as Entity;

				if ((entity != null) && (entity.Id == entityId))
				{
					return i;
				}
			}
			return -1;
		}

		// Poziva se iz MainWindowViewModel-a
		public void UpdateEntityOnCanvas(Entity entity)
		{
			int canvasIndex = GetCanvasIndexForEntityId(entity.Id);

			if (canvasIndex != -1)
			{
				if (entity.IsValueValidForType())
				{
					BorderBrushCollection[canvasIndex] = Brushes.Green;
				}
				else
				{
					BorderBrushCollection[canvasIndex] = Brushes.Red;
				}
			}
		}
	}
}
