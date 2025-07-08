using Rainbow;
using Rainbow.Model;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

public class SearchResultsView: View
{
    private const int typingDelay = 300; // in ms
    public const String LBL_SEARCH_IN_PROGRESS = "Searching ...";
    public const String LBL_NO_RESULT = "No result";

    public const String LBL_MY_NETWORK = "MY NETWORK";
    public const String LBL_MY_COMPANY = "MY COMPANY";
    public const String LBL_OTHER_COMPANIES = "CONTACTS OF OTHER COMPANIES";
    public const String LBL_PERSONAL_DIRECTORY = "PERSONAL DIRECTORY";
    public const String LBL_OTHER_DIRECTORIES = "OTHER DIRECTORIES";

    private readonly Object lockDisplay;
    private readonly ScrollableView scrollableView;
    private readonly Label lblMyNetwork;
    private readonly Label lblMyCompany;
    private readonly Label lblPersonalDirectory;
    private readonly Label lblOtherCompanies;
    private readonly Label lblOtherDirectories;
    private readonly Label lblSearchInProgress;

    private readonly Dictionary<String, SearchResultItem> searchResultItemList;

    private Boolean? searchInProgress = null;
    private String? searchValue = null;
    private List<ContactFound>? searchContactsResult;
    private List<String>? contactsIdToAvoid;

    private readonly Rainbow.Application rbApplication;
    private readonly Contacts rbContacts;
    private CancelableDelay? cancelableDelay;
    private String? currentContactId;
    private String? currentCompanyId;

    public event EventHandler<PeerAndMouseEventArgs>? PeerClick;

    public SearchResultsView(Rainbow.Application application)
    {
        rbApplication = application;
        rbContacts = rbApplication.GetContacts();

        searchResultItemList = [];
        lockDisplay = new();

        scrollableView = new();
        scrollableView.VerticalScrollBar.AutoShow = true;

        lblSearchInProgress = new()
        {
            X = Pos.Center(),
            Y = 0,
            Text = LBL_SEARCH_IN_PROGRESS,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
        };

        lblMyNetwork = new()
        {
            X = 0,
            Y = 0,
            Text = LBL_MY_NETWORK,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
        };

        lblMyCompany = new()
        {
            X = 0,
            Y = 1,
            Text = LBL_MY_COMPANY,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
        };

        lblPersonalDirectory = new()
        {
            X = 0,
            Y = 1,
            Text = LBL_PERSONAL_DIRECTORY,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
        };

        lblOtherCompanies = new()
        {
            X = 0,
            Y = 2,
            Text = LBL_OTHER_COMPANIES,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
        };

        lblOtherDirectories = new()
        {
            X = 0,
            Y = 3,
            Text = LBL_OTHER_DIRECTORIES,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = 1,
        };

        scrollableView.Add(lblSearchInProgress, lblMyNetwork, lblMyCompany, lblOtherCompanies, lblOtherDirectories);
        Add(scrollableView);

        Width = Dim.Fill();
        Height = Dim.Fill();

        BorderStyle = LineStyle.Dotted;
        CanFocus = true;

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        lock (lockDisplay)
        {
            // Copy Contacts Id to avoid
            List<string> contactsId = contactsIdToAvoid is null ? new() : new(contactsIdToAvoid);

            // Set default display / value
            lblSearchInProgress.Height = 0;
            lblMyNetwork.Height = 0;
            lblMyNetwork.Y = 0;
            lblMyCompany.Height = 0;
            lblMyCompany.Y = 0;
            lblPersonalDirectory.Height = 0;
            lblPersonalDirectory.Y = 0;
            lblOtherCompanies.Height = 0;
            lblOtherCompanies.Y = 0;
            lblOtherDirectories.Height = 0;
            lblOtherDirectories.Y = 0;

            // Remove previous Items
            foreach (var searchResultItem in searchResultItemList.Values.Reverse())
            {
                searchResultItem.Y = 0;
                searchResultItem.Height = 0;
                searchResultItem.PeerClick -= SearchResultItem_PeerClick;
                Remove(searchResultItem);
            }
            searchResultItemList.Clear();

            // Reset Viewport of scrollableView
            scrollableView.Viewport = scrollableView.Viewport with { Y = 0 };

            if (searchInProgress is null)
            {
                scrollableView.SetNbVerticalElements(1);
                // Something else to do ?
            }
            else if (searchInProgress.Value)
            {
                lblSearchInProgress.Height = 1;
                lblSearchInProgress.Text = LBL_SEARCH_IN_PROGRESS;
            }
            else
            {
                if (searchContactsResult is null)
                {
                    scrollableView.SetNbVerticalElements(1);
                    // Something else to do ?
                }
                else if (searchContactsResult.Count == 0)
                {
                    scrollableView.SetNbVerticalElements(1);
                    lblSearchInProgress.Height = 1;
                    lblSearchInProgress.Text = LBL_NO_RESULT;
                    // Something else to do ?
                }
                else
                {
                    int nbElements = 0;

                    var inMyNetWork = searchContactsResult
                                .Where(c => c.InRoster // In My Network
                                    && !contactsId.Contains(c.Peer.Id)) // Avoid some Id contacts
                                .ToList();
                    // Add Contacts Id
                    contactsId.AddRange(inMyNetWork.Select(c => c.Peer.Id));

                    var inMyCompany = searchContactsResult
                                .Where(c => c.CompanyId == currentCompanyId && c.Type == "Contact" // In My Company
                                    && !contactsId.Contains(c.Peer.Id)) // Avoid some Id contacts
                                .ToList();
                    // Add Contacts Id
                    contactsId.AddRange(inMyCompany.Select(c => c.Peer.Id));

                    var notInMyCompany = searchContactsResult
                                .Where(c => c.CompanyId != currentCompanyId && c.Type == "Contact" // NOT In My Company
                                    && !contactsId.Contains(c.Peer.Id)) // Avoid some Id contacts
                                .ToList();
                    // Add Contacts Id
                    contactsId.AddRange(notInMyCompany.Select(c => c.Peer.Id));

                    var personalDirectory = searchContactsResult
                                .Where(c => c.Type == "DirectoryContact" && c.SubType == "user"
                                    && !contactsId.Contains(c.Peer.Id)) // Avoid some Id contacts
                                .ToList();
                    // Add Contacts Id
                    contactsId.AddRange(personalDirectory.Select(c => c.Peer.Id));

                    // Get all other contacts
                    var otherDirectories = searchContactsResult
                                .Where(c => !contactsId.Contains(c.Peer.Id)) // Avoid some Id contacts
                                .ToList();

                    View? previousView = null;

                    // Manage inMyNetWork results
                    if (inMyNetWork.Count > 0)
                    {
                        lblMyNetwork.Height = 1;
                        if (previousView is null)
                        {
                            lblMyNetwork.Y = 0;
                            nbElements++;
                        }
                        else
                        {
                            lblMyNetwork.Y = Pos.Bottom(previousView) + 1;
                            nbElements += 2;
                        }
                        previousView = lblMyNetwork;

                        foreach (var contact in inMyNetWork)
                        {
                            var newView = AddSearchResultItem(previousView, contact);
                            if (newView is not null)
                            {
                                nbElements++;
                                previousView = newView;
                            }
                        }
                    }

                    // Manage inMyCompany results
                    if (inMyCompany.Count > 0)
                    {
                        lblMyCompany.Height = 1;
                        if (previousView is null)
                        {
                            lblMyCompany.Y = 0;
                            nbElements++;
                        }
                        else
                        {
                            lblMyCompany.Y = Pos.Bottom(previousView) + 1;
                            nbElements += 2;
                        }
                        previousView = lblMyCompany;

                        foreach(var contact in inMyCompany)
                        {
                            var newView = AddSearchResultItem(previousView, contact);
                            if (newView is not null)
                            {
                                nbElements++;
                                previousView = newView;
                            }
                        }
                    }

                    // Manage personalDirectory results
                    if (personalDirectory.Count > 0)
                    {
                        lblPersonalDirectory.Height = 1;
                        if (previousView is null)
                        {
                            lblPersonalDirectory.Y = 0;
                            nbElements++;
                        }
                        else
                        {
                            lblPersonalDirectory.Y = Pos.Bottom(previousView) + 1;
                            nbElements += 2;
                        }
                        previousView = lblPersonalDirectory;

                        foreach (var contact in personalDirectory)
                        {
                            var newView = AddSearchResultItem(previousView, contact);
                            if (newView is not null)
                            {
                                nbElements++;
                                previousView = newView;
                            }
                        }
                    }

                    // Manage notInMyCompany results
                    if (notInMyCompany.Count > 0)
                    {
                        lblOtherCompanies.Height = 1;
                        if (previousView is null)
                        {
                            lblOtherCompanies.Y = 0;
                            nbElements++;
                        }
                        else
                        {
                            lblOtherCompanies.Y = Pos.Bottom(previousView) + 1;
                            nbElements += 2;
                        }
                        previousView = lblOtherCompanies;

                        foreach (var contact in notInMyCompany)
                        {
                            var newView = AddSearchResultItem(previousView, contact);
                            if (newView is not null)
                            {
                                nbElements++;
                                previousView = newView;
                            }
                        }
                    }

                    if (otherDirectories.Count > 0)
                    {
                        lblOtherDirectories.Height = 1;
                        if (previousView is null)
                        {
                            lblOtherDirectories.Y = 0;
                            nbElements++;
                        }
                        else
                        {
                            lblOtherDirectories.Y = Pos.Bottom(previousView) + 1;
                            nbElements += 2;
                        }
                        previousView = lblOtherDirectories;
                        
                        foreach (var contact in otherDirectories)
                        {
                            var newView = AddSearchResultItem(previousView, contact);
                            if (newView is not null)
                            {
                                nbElements++;
                                previousView = newView;
                            }
                        }
                    }

                    scrollableView.SetNbVerticalElements(nbElements);

                    SetNeedsDraw();
                }
            }
        }
    }

    private SearchResultItem? AddSearchResultItem(View previousView, Contact contact)
    {
        var view = new SearchResultItem(rbApplication, contact)
        {
            X = 0,
            Y = Pos.Bottom(previousView),
            Width = Dim.Fill(),
            Height = 1
        };
        view.PeerClick += SearchResultItem_PeerClick;
        scrollableView.Add(view);
        searchResultItemList.TryAdd(contact.Peer.Id, view);
        return view;
    }

    public void SearchMembersByName(String name, List<String>? contactsIdToAvoid_ = null)
    {
        currentCompanyId = rbContacts.GetCompany()?.Id;
        currentContactId = rbContacts.GetCurrentContact()?.Peer?.Id;

        // Add current contact Id
        contactsIdToAvoid = contactsIdToAvoid_;
        contactsIdToAvoid ??= [];
        if (currentContactId is not null && !contactsIdToAvoid.Contains(currentContactId))
            contactsIdToAvoid.Add(currentContactId);

        searchValue = name;

        cancelableDelay?.Cancel();

        if (String.IsNullOrEmpty(name))
        {
            searchInProgress = null;
            Terminal.Gui.App.Application.Invoke(() =>
            {
                UpdateDisplay();
            });
        }
        else
        {
            cancelableDelay = CancelableDelay.StartAfter(typingDelay, StartSearchMembersByNameAsync);
        }
    }

    async private Task StartSearchMembersByNameAsync()
    {
        searchInProgress = true;
        searchContactsResult = null;
        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay();
        });

        // Start the search
        var sdkResult = await rbContacts.SearchByDisplayNameAsync(searchValue, 20);
        searchInProgress = false;
        searchContactsResult = sdkResult.Success ? sdkResult.Data : null;
        //TODO - enhance display in case there is an error in search 

        Terminal.Gui.App.Application.Invoke(() =>
        {
            UpdateDisplay();
        });
    }

    private void SearchResultItem_PeerClick(object? sender, PeerAndMouseEventArgs e)
    {
        PeerClick?.Invoke(this, e);
    }
}

