export class Keyword {
  constructor(name, description = '', hasImage = false, synonyms = [], related = []) {
    this.name = name;
    this.description = description;
    this.hasImage = hasImage;
    this.synonyms = synonyms;
    this.related = related;
  }

  static fromJson(jsonData) {
    return new Keyword(
      jsonData.Name,
      jsonData.Description || '',
      jsonData.HasImage || false,
      jsonData.Synonyms || [],
      jsonData.Related || []
    );
  }
} 