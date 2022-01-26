using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
	public class NetworkEntitiesViewModel : BindableBase
	{
		public List<string> ComboBoxItems { get; set; } = Data.ComboBoxItemsData.entityTypes.Keys.ToList();

		public ObservableCollection<Entity> EntitiesToShow { get; set; }
		public ObservableCollection<Entity> Entities { get; set; }
		public ObservableCollection<Entity> FilteredEntities { get; set; }

		public MyICommand AddEntityCommand { get; set; }
		public MyICommand DeleteEntityCommand { get; set; }
		public MyICommand FilterEntityCommand { get; set; }
		public MyICommand ResetFilterCommand { get; set; }

		// Unos novog entiteta
		private Entity currentEntity = new Entity();
		private EntityType currentEntityType = new EntityType();

		// Entitet selektovan u datagridu
		private Entity selectedEntity;

		// Filtriranje
		private string selectedEntityTypeToFilter;
		private bool isGreaterThanRadioButtonSelected;
		private bool isLessThanRadioButtonSelected;
		private string selectedIdMarginToFilterText;
		private bool isOutOfRangeValuesRadioButtonSelected;
		private bool isExpectedValuesRadioButtonSelected;
		private string filterErrorMessage;

		// Prikaz broja entiteta po tipu
		private int iaRectangleWidth;
		private int iaCount = 0;
		private string iaPercentage;
		private int ibRectangleWidth;
		private int ibCount = 0;
		private string ibPercentage;

		public NetworkEntitiesViewModel()
		{
			Entities = new ObservableCollection<Entity>();
			EntitiesToShow = Entities;

			LoadDefaultValuesForDisplay();

			AddEntityCommand = new MyICommand(OnAdd);
			DeleteEntityCommand = new MyICommand(OnDelete, CanDelete);
			FilterEntityCommand = new MyICommand(OnFilter);
			ResetFilterCommand = new MyICommand(OnResetFilter);
		}

		private void LoadDefaultValuesForDisplay()
		{
			IARectangleWidth = 200;
			IBRectangleWidth = 200;

			IAPercentage = "50% (0)";
			IBPercentage = "50% (0)";
		}

		public Entity SelectedEntity
		{
			get { return selectedEntity; }
			set
			{
				selectedEntity = value;
				DeleteEntityCommand.RaiseCanExecuteChanged();
			}
		}

		public string SelectedEntityTypeToFilter
		{
			get { return selectedEntityTypeToFilter; }
			set
			{
				selectedEntityTypeToFilter = value;
				OnPropertyChanged("SelectedEntityTypeToFilter");
			}
		}

		public bool IsGreaterThanRadioButtonSelected
		{
			get { return isGreaterThanRadioButtonSelected; }
			set
			{
				isGreaterThanRadioButtonSelected = value;
				OnPropertyChanged("IsGreaterThanRadioButtonSelected");
			}
		}

		public bool IsLessThanRadioButtonSelected
		{
			get { return isLessThanRadioButtonSelected; }
			set
			{
				isLessThanRadioButtonSelected = value;
				OnPropertyChanged("IsLessThanRadioButtonSelected");
			}
		}

		public string SelectedIdMarginToFilterText
		{
			get { return selectedIdMarginToFilterText; }
			set
			{
				selectedIdMarginToFilterText = value;
				OnPropertyChanged("SelectedIdMarginToFilterText");
			}
		}

		public bool IsOutOfRangeValuesRadioButtonSelected
		{
			get { return isOutOfRangeValuesRadioButtonSelected; }
			set
			{
				isOutOfRangeValuesRadioButtonSelected = value;
				OnPropertyChanged("IsOutOfRangeValuesRadioButtonSelected");
			}
		}

		public bool IsExpectedValuesRadioButtonSelected
		{
			get { return isExpectedValuesRadioButtonSelected; }
			set
			{
				isExpectedValuesRadioButtonSelected = value;
				OnPropertyChanged("IsExpectedValuesRadioButtonSelected");
			}
		}

		public string FilterErrorMessage
		{
			get { return filterErrorMessage; }
			set
			{
				filterErrorMessage = value;
				OnPropertyChanged("FilterErrorMessage");
			}
		}

		private void OnFilter()
		{
			FilterErrorMessage = String.Empty;

			if (SelectedEntityTypeToFilter == null &&
				!IsGreaterThanRadioButtonSelected &&
				!IsLessThanRadioButtonSelected &&
				string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText) &&
				!IsOutOfRangeValuesRadioButtonSelected &&
				!IsExpectedValuesRadioButtonSelected)
			{
				FilterErrorMessage = "Fields can't be empty.";
				return;
			}

			List<Entity> filteredList = new List<Entity>();

			foreach (Entity entity in Entities)
			{
				filteredList.Add(entity);
			}

			if (SelectedEntityTypeToFilter != null)
			{
				List<Entity> entitiesToDelete = new List<Entity>();
				for (int i = 0; i < filteredList.Count; i++)
				{
					if (filteredList[i].Type.Name != SelectedEntityTypeToFilter)
					{
						Console.WriteLine($"Podudaranje na indeksu {i}");
						entitiesToDelete.Add(filteredList[i]);
					}
				}
				
				foreach (Entity entity in entitiesToDelete)
				{
					filteredList.Remove(entity);
				}
			}

			if (IsGreaterThanRadioButtonSelected)
			{
				List<Entity> entitiesToDelete = new List<Entity>();

				if (string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText))
				{
					FilterErrorMessage = "Id is required.";
				}
				else
				{
					int selectedId;
					bool parsingSuccessful = int.TryParse(SelectedIdMarginToFilterText, out selectedId);

					if (parsingSuccessful)
					{
						FilterErrorMessage = String.Empty;

						if (selectedId >= 0)
						{
							FilterErrorMessage = String.Empty;

							for (int i = 0; i < filteredList.Count; i++)
							{
								if (filteredList[i].Id <= selectedId)
								{
									entitiesToDelete.Add(filteredList[i]);
								}
							}

							foreach (Entity entity in entitiesToDelete)
							{
								filteredList.Remove(entity);
							}
						}
						else
						{
							FilterErrorMessage = "Id can't be negative.";
						}
					}
					else
					{
						FilterErrorMessage = "Id must be an integer.";
					}
				}
			}
			else if (IsLessThanRadioButtonSelected)
			{
				List<Entity> entitiesToDelete = new List<Entity>();

				if (string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText))
				{
					FilterErrorMessage = "Id is required.";
				}
				else
				{
					int selectedId;
					bool parsingSuccessful = int.TryParse(SelectedIdMarginToFilterText, out selectedId);

					if (parsingSuccessful)
					{
						FilterErrorMessage = String.Empty;

						if (selectedId >= 0)
						{
							FilterErrorMessage = String.Empty;

							for (int i = 0; i < filteredList.Count; i++)
							{
								if (filteredList[i].Id >= selectedId)
								{
									entitiesToDelete.Add(filteredList[i]);
								}
							}

							foreach (Entity entity in entitiesToDelete)
							{
								filteredList.Remove(entity);
							}
						}
						else
						{
							FilterErrorMessage = "Id can't be negative.";
						}
					}
					else
					{
						FilterErrorMessage = "Id must be an integer.";
					}
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText))
				{
					FilterErrorMessage = "Select '>' or '<'.";
				}
			}

			if (IsOutOfRangeValuesRadioButtonSelected)
			{
				List<Entity> entitiesToDelete = new List<Entity>();

				for (int i = 0; i < filteredList.Count; i++)
				{
					if (filteredList[i].IsValueValidForType())
					{
						entitiesToDelete.Add(filteredList[i]);
					}
				}

				foreach (Entity entity in entitiesToDelete)
				{
					filteredList.Remove(entity);
				}
			}
			else if (IsExpectedValuesRadioButtonSelected)
			{
				List<Entity> entitiesToDelete = new List<Entity>();

				for (int i = 0; i < filteredList.Count; i++)
				{
					if (!filteredList[i].IsValueValidForType())
					{
						entitiesToDelete.Add(filteredList[i]);
					}
				}

				foreach (Entity entity in entitiesToDelete)
				{
					filteredList.Remove(entity);
				}
			}

			if (filteredList.Count > 0)
			{
				FilteredEntities = new ObservableCollection<Entity>();

				foreach (Entity entity in filteredList)
				{
					FilteredEntities.Add(entity);
				}

				EntitiesToShow = FilteredEntities;
				OnPropertyChanged("EntitiesToShow");
			}
			else
			{
				FilterErrorMessage = "No entities to show.";
				EntitiesToShow = Entities;
				OnPropertyChanged("EntitiesToShow");
			}
		}

		private void OnResetFilter()
		{
			SelectedEntityTypeToFilter = null;
			IsGreaterThanRadioButtonSelected = false;
			IsLessThanRadioButtonSelected = false;
			SelectedIdMarginToFilterText = String.Empty;
			IsOutOfRangeValuesRadioButtonSelected = false;
			IsExpectedValuesRadioButtonSelected = false;
			FilterErrorMessage = String.Empty;

			EntitiesToShow = Entities;
			FilteredEntities = new ObservableCollection<Entity>();
			OnPropertyChanged("EntitiesToShow");
		}

		private bool CanDelete()
		{
			return SelectedEntity != null;
		}

		private void OnDelete()
		{
			switch (SelectedEntity.Type.Name)
			{
				case "IA":
					iaCount--;
					break;
				case "IB":
					ibCount--;
					break;
			}
			
			Entities.Remove(SelectedEntity);

			OnDataGridUpdate();
		}

		public Entity CurrentEntity
		{
			get { return currentEntity; }
			set
			{
				currentEntity = value;
				OnPropertyChanged("CurrentEntity");
			}
		}

		public EntityType CurrentEntityType
		{
			get { return currentEntityType; }
			set
			{
				currentEntityType = value;
				OnPropertyChanged("CurrentEntityType");
			}
		}

		private void OnDataGridUpdate()
		{
			if (Entities.Count > 0)
			{
				int tempIaPercentage = iaCount * 100 / (iaCount + ibCount);
				int tempIbPercentage = ibCount * 100 / (iaCount + ibCount);

				IAPercentage = $"{tempIaPercentage}% ({iaCount})";
				IBPercentage = $"{tempIbPercentage}% ({ibCount})";

				if (tempIaPercentage == 100)
				{
					IARectangleWidth = 400 * tempIaPercentage / 100;
					IBRectangleWidth = 400 - IARectangleWidth;
					IBPercentage = "";
				}
				else if (tempIbPercentage == 100)
				{
					IARectangleWidth = 400 * tempIaPercentage / 100;
					IBRectangleWidth = 400 - IARectangleWidth;
					IAPercentage = "";
				}
				else
				{
					IARectangleWidth = 400 * tempIaPercentage / 100;
					IBRectangleWidth = 400 - IARectangleWidth;
				}
			}
			else
			{
				LoadDefaultValuesForDisplay();
			}
			
		}

		public void OnAdd()
		{
			try
			{
				int parsedId;
				bool canParse = int.TryParse(CurrentEntity.TextId, out parsedId);
				bool idAlreadyExists = false;

				if (canParse)
				{
					foreach (Entity entity in Entities)
					{
						if (entity.Id == parsedId)
						{
							idAlreadyExists = true;
							break;
						}
					}
				}

				CurrentEntity.DoesIdAlreadyExist = idAlreadyExists;

				CurrentEntity.Validate();
				CurrentEntityType.Validate();

				if (CurrentEntity.IsValid)
				{
					Entities.Add(new Entity()
					{
						Id = int.Parse(CurrentEntity.TextId),
						Name = CurrentEntity.Name,
						Type = new EntityType
						{
							Name = CurrentEntityType.Name,
							ImgSrc = CurrentEntityType.ImgSrc
						},
						Value = 0
					});

					switch (CurrentEntityType.Name)
					{
						case "IA":
							iaCount++;
							break;
						case "IB":
							ibCount++;
							break;
					}

					OnDataGridUpdate();

					CurrentEntity.TextId = String.Empty;
					CurrentEntity.Name = String.Empty;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{DateTime.Now} - {ex.Message}");
			}
		}

		public int IARectangleWidth
		{
			get { return iaRectangleWidth; }
			set
			{
				iaRectangleWidth = value;
				OnPropertyChanged("IARectangleWidth");
			}
		}

		public string IAPercentage
		{
			get { return iaPercentage; }
			set
			{
				iaPercentage = value;
				OnPropertyChanged("IAPercentage");
			}
		}

		public int IBRectangleWidth
		{
			get { return ibRectangleWidth; }
			set
			{
				ibRectangleWidth = value;
				OnPropertyChanged("IBRectangleWidth");
			}
		}

		public string IBPercentage
		{
			get { return ibPercentage; }
			set
			{
				ibPercentage = value;
				OnPropertyChanged("IBPercentage");
			}
		}
	}
}
