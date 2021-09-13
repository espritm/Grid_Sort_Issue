using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Sort_Grid
{
    public partial class MainPage : ContentPage
    {
        private int _nbColumn = 3;
        private int _nbRow = 3;

        Grid Grid;
        List<Item> Cards = new List<Item>();
        Item _itemBeingDragged;

        public MainPage()
        {
            InitializeComponent();


            //Create data
            for (int i = 0; i < _nbRow; i++)
                for (int j = 0; j < _nbColumn; j++)
                    Cards.Add(new Item()
                    {
                        ID = Cards.Count,
                        Title = $"Tile {Cards.Count}",
                        Row = i,
                        RowSpan = 1,
                        Column = j,
                        ColumnSpan = 1
                    });


            Content = Grid = new Grid
            {
                Padding = 20,
                ColumnSpacing = 20,
                RowSpacing = 10,
            };

            for (int i = 0; i < _nbRow; i++)
                Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            for (int i = 0; i < _nbColumn; i++)
                Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


            //Populate Grid with data
            RedrawGrid();


            //Add dropGesture to Grid to allow user to cancel a drag and drop gesture
            DropGestureRecognizer dropGesture = new DropGestureRecognizer();
            dropGesture.Drop += DropGesture_Drop;

            Grid.GestureRecognizers.Add(dropGesture);
        }

        private void RedrawGrid()
        {
            Grid.Children.Clear();

            //Verify there is no card going out of the _nbRow x _nbColumn grid.
            foreach (Item card in Cards)
            {
                if (card.Row + card.RowSpan > _nbRow)
                    card.RowSpan = Math.Min(_nbRow - card.Row, card.Row + card.RowSpan + 1);

                if (card.Column + card.ColumnSpan > _nbColumn)
                    card.ColumnSpan = Math.Min(_nbColumn - card.Column, card.Column + card.ColumnSpan + 1);
            }

            //Remove items hidden by rowspan and/or columnspan
            List<Item> tmp = new List<Item>();
            tmp.AddRange(Cards);

            foreach (Item card in tmp)
            {
                if (card.RowSpan > 1)
                    Cards.RemoveAll(c => c.Row > card.Row && c.Row < card.Row + card.RowSpan && c.Column >= card.Column && c.Column < card.Column + card.ColumnSpan);

                if (card.ColumnSpan > 1)
                    Cards.RemoveAll(c => c.Column > card.Column && c.Column < card.Column + card.ColumnSpan && c.Row >= card.Row && c.Row < card.Row + card.RowSpan);
            }

            //display data into the grid
            foreach (Item card in Cards)
                Grid.Children.Add(GetItemView(card), card.Column, card.Column + card.ColumnSpan, card.Row, card.Row + card.RowSpan);
        }

        public Frame GetItemView(Item item)
        {
            Frame frame = new Frame
            {
                HasShadow = false,
                Padding = 0,
                BorderColor = Color.Black,
                Content = new StackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(10, 10, 10, 0),
                    Children =
                    {
                        new Label
                        {
                            Text = item.Title,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                        },
                        new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Spacing = 30,
                            Children =
                            {
                                new Button
                                {
                                    Text = "Decr RowSpan",
                                    Command = new Command (() =>
                                    {
                                        item.RowSpan = Math.Max(1, item.RowSpan - 1);
                                        RedrawGrid();
                                    })
                                },
                                new Button
                                {
                                    Text = "Incr RowSpan",
                                    Command = new Command (() =>
                                    {
                                        item.RowSpan++;// = Math.Min(_nbRow - item.Row, item.Row + item.RowSpan + 1); verification are done in RedrawGrid.
                                        RedrawGrid();
                                    })
                                }
                            }
                        },
                        new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Spacing = 30,
                            Children =
                            {
                                new Button
                                {
                                    Text = "Decr ColSpan",
                                    Command = new Command (() =>
                                    {
                                        item.ColumnSpan = Math.Max(1, item.ColumnSpan - 1);
                                        RedrawGrid();
                                    })
                                },
                                new Button
                                {
                                    Text = "Incr ColSpan",
                                    Command = new Command (() =>
                                    {
                                        item.ColumnSpan++;// = Math.Min(_nbColumn - item.Column, item.Column + item.ColumnSpan + 1); verification are done in RedrawGrid.
                                        RedrawGrid();
                                    })
                                }
                            }
                        },
                        new Button
                        {
                            Text = "Remove",
                            Command = new Command (() =>
                            {
                                Cards.Remove(item);
                                RedrawGrid();
                            })
                        }
                    }
                }
            };

            DragGestureRecognizer dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, args) =>
            {
                _itemBeingDragged = item;
            };

            DropGestureRecognizer dropGesture = new DropGestureRecognizer();
            dropGesture.DragOver += (s, args) =>
            {
                if (item == null || item.ID == _itemBeingDragged?.ID)
                    return;

                int prevRow = _itemBeingDragged.Row;
                int prevCol = _itemBeingDragged.Column;


                //Swipe the two items
                InvertTwoGridItem(_itemBeingDragged, item);



                //Populate Grid with data
                RedrawGrid();
            };

            frame.GestureRecognizers.Add(dragGesture);
            frame.GestureRecognizers.Add(dropGesture);

            return frame;
        }

        private void InvertTwoGridItem(Item item1, Item item2)
        {
            int prevRow = item1.Row;
            int prevCol = item1.Column;


            item1.Row = item2.Row + item2.RowSpan - 1;
            item1.Column = item2.Column + item2.ColumnSpan - 1;


            item2.Row = prevRow + item1.RowSpan - 1;
            item2.Column = prevCol + item1.ColumnSpan - 1;


            //If swiped item1 had rowSpan, move all cards covered by the rowspan if a free space is available
            if (item1.RowSpan > 1)
            {
                List<Item> cards = Cards.FindAll(c => c.Row > item1.Row && c.Row < item1.Row + item1.RowSpan && c.Column >= item1.Column && c.Column < item1.Column + item1.ColumnSpan);

                //Swipe all cards overriden by item1's rowspan if there is enought place. If not, will be removed in RedrawGrid
                foreach (Item card in cards)
                {
                    if (Cards.Find(c => c.Row == card.Row && c.Column == item2.Column) == null)
                        card.Column = item2.Column;
                }
            }

            //Is swiped item1 had columnspan, move all cards covered by the columnspan if a free space is available
            if (item1.ColumnSpan > 1)
            {
                List<Item> cards = Cards.FindAll(c => c.Column > item1.Column && c.Column < item1.Column + item1.ColumnSpan && c.Row >= item1.Row && c.Row < item1.Row + item1.RowSpan);

                foreach (Item card in cards)
                {
                    if (Cards.Find(c => c.Column == card.Column && c.Row == item2.Row) == null)
                        card.Row = item2.Row;
                }
            }



            //If swiped item2 had rowSpan, move all cards covered by the rowspan if a free space is available
            if (item2.RowSpan > 1)
            {
                List<Item> cards = Cards.FindAll(c => c.Row > item2.Row && c.Row < item2.Row + item2.RowSpan && c.Column >= item2.Column && c.Column < item2.Column + item2.ColumnSpan);

                //Swipe all cards overriden by item2's rowspan if there is enought place. If not, will be removed in RedrawGrid
                foreach (Item card in cards)
                {
                    if (Cards.Find(c => c.Row == card.Row && c.Column == item1.Column) == null)
                        card.Column = item1.Column;
                }
            }

            //Is swiped item2 had columnspan, move all cards covered by the columnspan if a free space is available
            if (item2.ColumnSpan > 1)
            {
                List<Item> cards = Cards.FindAll(c => c.Column > item2.Column && c.Column < item2.Column + item2.ColumnSpan && c.Row >= item2.Row && c.Row < item2.Row + item2.RowSpan);

                foreach (Item card in cards)
                {
                    if (Cards.Find(c => c.Column == card.Column && c.Row == item1.Row) == null)
                        card.Row = item1.Row;
                }
            }
        }

        private void DropGesture_Drop(object sender, DropEventArgs e)
        {
            ;//Nothing to do
        }

        public class Item
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public int Row { get; set; }
            public int RowSpan { get; set; }
            public int Column { get; set; }
            public int ColumnSpan { get; set; }
        }
    }
}
