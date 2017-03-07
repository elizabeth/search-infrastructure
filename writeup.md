# Write-up
This search infrastructure aims to tie together my previous INFO 344 assignments in order to create an search engine with search suggestions from wiki, and shows NBA player stats and results from CNN and Bleacher Report.

Upon searching, query suggestions are shown based on data from my Microsoft Azure Storage Blob using AJAX. This blob contains a file of given Wikipedia pages (from dumps.wikimedia.org) which has been downloaded to the Cloud Service the app is hosted on. From this file, a Trie is built of all the words and used to search through to retrieve query suggestions.

Hitting the search button (or enter) saves the search as a suggestion that will return first when a user searches for even just the beginning of the search term.

When a user searches an NBA player, such as `Stephen Curry`, I retrieve the data about the player from my AWS RDS and display his stats. I make an AJAX call using JSONP to a program hosted on my AWS which calls my RDS to get the data and then return it. I have to use JSONP in order to make a cross-domain call. If a search does not match an NBA player, I do not show  stats but only search results relevant to the search query, which is explained further down in this document. 

Searches also return relevant results from crawled webpages. Information about crawled webpages are stored in a Microsoft Azure Table. When a user searches, I go through this table to return any relevant results (up to 20). Results are found based on the words in the title of the page and ranked according to the number of matched words and date. In my table, a url is mapped to the words in the title. Any punctuation is stripped from the title and only exact matches (ignoring case) are done. Results returned show the title, link, date, and the description (if any is set by the page - otherwise it is set as the title). Words from the search are bolded in the page description so that users can easily see the words that they searched for.

In order to allow users to search for words and return any relevant pages based on the words in a page's title, I store each page I crawl multiple times in my table. Each page is mapped to a word in its title which is how I am able to retrieve and then rank search results to show to the user.

To have searching be just a bit faster, I cache every 100 (different) searches so that when a user searches for something that was recently previously searched, I can instantly retrieve a list of the already sorted results rather than taking the time to go through the table and retrieve and order all the results again.

Like any website that needs money, I integrate Google adsense into my website. The ad is kept and shown at the bottom of the screen. 