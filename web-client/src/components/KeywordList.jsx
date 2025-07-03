import React, { useEffect, useState } from 'react';
import KeywordBox from './KeywordBox';
import { getCachedCardKeyword } from '../utils/cardDataCache.js';
import './KeywordList.css';

function KeywordList({ keywordNames }) {
  const [keywords, setKeywords] = useState([]);

  useEffect(() => {
    let isMounted = true;
    Promise.all(keywordNames.map(name => getCachedCardKeyword(name))).then(loadedKeywords => {
      if (isMounted) setKeywords(loadedKeywords);
    });
    return () => { isMounted = false; };
  }, []); // Only run on mount

  return (
    <div className="keyword-list">
      {keywords.map((keyword, idx) => (
        <div className="keyword-list-item" key={keyword.name || idx}>
          <KeywordBox keyword={keyword} style={{ width: '100%' }} imageHeight={50} />
        </div>
      ))}
    </div>
  );
}

export default KeywordList; 