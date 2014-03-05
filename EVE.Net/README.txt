
Some notes:

This library was designed as a resource to read data provided by the EVE Online API, which is documented here: http://wiki.eveonline.com/en/wiki/Category:API
The EVE.Net libary is released under the MIT License, details of which are included in the file 'license.txt'

The classes provided are intended to be a one-to-one mapping to the various API calls available in the EVE API.  They do not provide any additional details or
do any mappings against the EVE database dumps.  Though I am considering writing a companion library which will do just that.

This library is compiled against .NET 2.0, and contains no additional references or dependencies.

Why write this when there are other libraries out there?  Good question.  Most of those libraries seem to be unsupported/dead code.  The one notable exception
is EveAI, an excellent library (http://wiki.eve-id.net/EveAI).  I like to have the source to libraries I consume however, so I wrote this one.  
(additional note: according to the EVEAI documentation, it does not contain all of the library calls.  This library does.)

Using this library is pretty straightforward.  Each class provides one or more constructors which accept the arguments that are necessary to make a successful
call against the API.  In all cases where API keys are required, it is expected that you will use the new API key format.  Use of the old keys has not been tested.
You can obtain your API keys from the key management page here: https://support.eveonline.com/api

Examples;
--------------------------------

AccountStatus accountStatus = new AccountStatus(mykeyID, myVcode, myCharID);
accountStatus.Query();

The 'Query' call will attempt to access the EVE API.

Another example...

         MarketOrders market = new MarketOrders(SelectedKey.ID, SelectedKey.vCode, SelectedKey.CharID);

         Console.WriteLine("Orders");
         foreach (MarketOrders.Order order in market.orders)
         {
            Console.WriteLine("ID{0}, STATION:{1}, ORIGVOL:{2}, VOLREMAINING:{3}", order.orderID, order.volEntered, order.volRemaining);
         }

Since all of the fields returned from the API result set are exposed as public properties, they should be easily bindable in WPF.

Once 'Query' is made successfully, then the results of the query will be available via public properties of the object.  'AccountStatus' exposes properties such
as 'userID', 'paidUntil', 'createDate', etc.

That's really all there is too it.  In those cases where the API call requires more or less arguments, the specific constructor should be available.  If I've missed
anything, please let me know.


Corporate Versions of API calls
--------------------------------

In all cases where there were both Character specific and Corporate specific versions of an API call, I have written one class that will handle both cases, and these
classes are all in the 'Character' sub-folder.  To use the character version, pass in a non-corporate key, and a corporate key for the corporate version.

The dual-purpose api classes are:

Character/AccountBalance.cs
Character/AssetList.cs
Character/ContractBids.cs
Character/ContractItems.cs
Character/IndustryJobs.cs
Character/KillMails.cs
Character/Locations.cs
Character/MarketOrders.cs
Character/NPCStandings.cs
Character/WalletJournal.cs
Character/WalletTransactions.cs

Caching
--------------------------------

Caching occurs automatically and obeys the cache times given by the EVE API.  Cache file names contain the api details required to make the calls unique.  
If this poses a security issue for you, well - I'm open to suggestions.

Settings / Options
--------------------------------

The 'Settings.cs' file contains static properties which you can modify to suit your needs.

Error Handling
--------------------------------

You can choose how to handle errors:
1) Fail gracefully. (default)
   The Query() function will return true on success or false on failure.
   It is up to you what you do with that.
2) Fail hard.
   The Query() function will throw an Exception on failure. (or simply return true on success)
Regardless of your choice, the specific error will be saved in the error variable.

Expanding / Adding New API calls
--------------------------------

While this library is, to date, complete with all known API calls.  Adding new ones is trivial. Simply write a new class which mimics the data structure,
using public properties, of the result set from the new API. Inherit from the APIObject base class, and implement the required overrides (simple: just follow
the examples of the existing apis).  The Query function will do the rest.  To be clear - you do not have to write xml parsing logic for each new class, that 
is done automatically by the 'APIReader'.

Design Decisions
--------------------------------

The way this library works, it expects the APIObject that is making the call to the EVE Api to contain the same basic structure that the API will return in
it's results set.  The library uses reflection to compare public properties of the object against the result set, and populates those properties appropriately.
Because of this, the library objects are perhaps not great representatives of an object oriented class.

Most of the library classes make use of various data transfer objects.  Early on, I attempted to organize these objects outside of the classes that needed them, and
attempted to share them between classes as much as possible.

This turned out to be a nightmare.  Many API calls have simmilar result sets, but few of them are actually identical.  To attempt to share data objects between classes
would have meant that properties would be populated in some API calls, but not others.

Further, trying to separate these data objects from the classes that consumed them seriously polluted the public-facing API.  

So, I ended up deciding to organize these data objects as nested classes within the consuming object.  Normally, I am not a fan of nested classes, but in this case
they worked out very well.

There are a few library classes that could share data objects, but I chose to duplicate those objects as nested classes instead. I did this for consistency only.

New API stuff
--------------------------------
CCP Prism X recently added entity type ID's to most API's.
I have added all the ID's you could possibly get returned from that to the EVEConstants class.
Simply check against those ID's to see which it is.
For example, if you have a MailMessages object called mm, you can do it as follows:

if (mm.senderTypeID == EVEConstants.corporationType)
or
if (EVEConstants.characterTypes.Contains(mm.senderTypeID)) 