export class CardTooltip {
  constructor(cardId, hasBlueprint = false, keywords = [], relatedCards = []) {
    this.cardId = cardId;
    this.hasBlueprint = hasBlueprint;
    this.keywords = keywords;
    this.relatedCards = relatedCards;
  }

  static fromJson(jsonData) {
    return new CardTooltip(
      jsonData.CardId,
      jsonData.HasBlueprint || false,
      jsonData.Keywords || [],
      jsonData.RelatedCards || []
    );
  }
} 