export function ProductArt({ code }: { code: string }) {
  return <svg viewBox="0 0 240 170" fill="none" aria-hidden="true" className="product-art">
    <ellipse cx="120" cy="144" rx="65" ry="8" fill="currentColor" opacity=".08" />
    {code === "P001" && <g stroke="currentColor" strokeWidth="2.5">
      <path d="M87 80c0-29 13-46 33-46s33 17 33 46v27c0 23-13 36-33 36s-33-13-33-36Z" fill="#f9faf5" />
      <path d="M120 34v48M88 83h64" />
      <rect x="116" y="53" width="8" height="19" rx="4" fill="currentColor" stroke="none" />
      <path d="M120 34v-9c0-10 8-14 21-14" strokeLinecap="round" />
    </g>}
    {code === "P002" && <g transform="rotate(-8 120 90)" stroke="currentColor" strokeWidth="2">
      <rect x="27" y="53" width="186" height="81" rx="10" fill="#f9faf5" />
      {[0, 1, 2].map(row => Array.from({ length: 10 }, (_, col) =>
        <rect key={`${row}-${col}`} x={38 + col * 16.6} y={64 + row * 16} width="11" height="10" rx="2" opacity=".7" />))}
      <rect x="75" y="114" width="80" height="10" rx="2" />
      <path d="M45 119h19m103 0h25" strokeLinecap="round" />
    </g>}
    {code === "P003" && <g stroke="currentColor" strokeWidth="3">
      <path d="M69 98V78c0-68 102-68 102 0v20" strokeWidth="12" strokeLinecap="round" />
      <path d="M76 80c0-57 88-57 88 0" stroke="#f9faf5" strokeWidth="4" />
      <rect x="57" y="83" width="30" height="53" rx="13" fill="#f9faf5" transform="rotate(-8 72 108)" />
      <rect x="153" y="83" width="30" height="53" rx="13" fill="#f9faf5" transform="rotate(8 168 108)" />
      <path d="M82 94v29m76-29v29" opacity=".5" />
    </g>}
    {code === "P004" && <g stroke="currentColor" strokeWidth="2.5">
      <rect x="42" y="26" width="156" height="102" rx="6" fill="#f9faf5" />
      <path d="M49 33h142v80H49z" fill="currentColor" opacity=".1" stroke="none" />
      <path d="m51 111 43-44 30 30 32-46 33 60" opacity=".4" />
      <path d="M112 128v15h-21m37-15v15h21M91 145h58" strokeLinecap="round" />
      <circle cx="120" cy="120" r="2" fill="currentColor" stroke="none" />
    </g>}
  </svg>;
}
