This directory contains **CardData** which are .json files that describe the cards mechanics.

To find something, the tree is the following:

CardData/CardPictures -> **\[expansion\]** -> **\[class\]**

There's also an *index.csv* file in base directory that correlates the card number (id) with the corresponding card type and expansion/class for the program to easily find the desired card.

- **CardImagesRaw** folder contains the square picture in each card, not separated by any Class/Expansion structure.
- **CardImagesFull** is the whole card graphics.
- **CardBlueprintsFull** is the blueprint card for buildings.
- **CardLayoutElements** is some .pngs of important card elements needed to fully draw the card (e.g. blueprint pattern, stat icons)
- **CardIllustrationData** contains exported .json but used for illustration generation and not for game engine