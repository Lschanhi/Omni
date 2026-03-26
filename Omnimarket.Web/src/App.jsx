import { useDeferredValue, useEffect, useMemo, useRef, useState, useTransition } from "react";
import { mockFeed } from "./data/mockFeed";

const PAGE_SIZE = 8;
const CATEGORY_ORDER = ["Tudo", "Eletronicos", "Moda", "Casa", "Esportes", "Games"];

function inferCategory(name, index) {
  const normalized = name.toLowerCase();

  if (normalized.includes("fone") || normalized.includes("drone") || normalized.includes("camera")) {
    return "Eletronicos";
  }

  if (normalized.includes("tenis") || normalized.includes("mochila")) {
    return "Moda";
  }

  if (normalized.includes("lamp") || normalized.includes("cadeira") || normalized.includes("desk")) {
    return "Casa";
  }

  if (normalized.includes("runner") || normalized.includes("band")) {
    return "Esportes";
  }

  if (normalized.includes("teclado") || normalized.includes("mouse") || normalized.includes("monitor")) {
    return "Games";
  }

  return CATEGORY_ORDER[(index % (CATEGORY_ORDER.length - 1)) + 1];
}

function calcDiscountPercent(oldPrice, price) {
  return Math.max(5, Math.round(((oldPrice - price) / oldPrice) * 100));
}

const enrichedFeed = mockFeed.map((item, index) => {
  const oldPrice = Number((item.price * (1.12 + (index % 3) * 0.05)).toFixed(2));
  const rating = Number((4.3 + ((index % 7) * 0.1)).toFixed(1));

  return {
    ...item,
    category: inferCategory(item.product, index),
    oldPrice,
    rating,
    reviews: 140 + index * 17,
    freeShipping: index % 2 === 0,
    full: index % 3 === 0,
    bestSeller: index % 5 === 0,
    lowStock: index % 7 === 0,
    discountPercent: calcDiscountPercent(oldPrice, item.price)
  };
});

const categories = CATEGORY_ORDER.filter(
  (category) => category === "Tudo" || enrichedFeed.some((item) => item.category === category)
);

function formatPrice(value) {
  return value.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

function SearchIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M10.8 4a6.8 6.8 0 1 1 0 13.6A6.8 6.8 0 0 1 10.8 4Zm5.6 12 4 4" />
    </svg>
  );
}

function BellIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 3a5 5 0 0 0-5 5v2.4c0 .9-.3 1.7-.9 2.4L4.6 14a1 1 0 0 0 .7 1.7H18.7a1 1 0 0 0 .7-1.7l-1.5-1.2a3.7 3.7 0 0 1-.9-2.4V8a5 5 0 0 0-5-5Z" />
      <path d="M10 18a2 2 0 0 0 4 0" />
    </svg>
  );
}

function CartIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M3.5 5h2l2 10h9l2-7H7.5" />
      <path d="M9 19a1.5 1.5 0 1 0 0 .1m8-0.1a1.5 1.5 0 1 0 0 .1" />
    </svg>
  );
}

function HomeIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M3 11.3 12 4l9 7.3V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1v-8.7Z" />
    </svg>
  );
}

function GridIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M4 4h7v7H4zM13 4h7v7h-7zM4 13h7v7H4zM13 13h7v7h-7z" />
    </svg>
  );
}

function HeartIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 20s-6.2-3.9-8.5-8A4.9 4.9 0 0 1 7.8 4c1.7 0 3.2.9 4.2 2.2A5.2 5.2 0 0 1 16.2 4a4.9 4.9 0 0 1 4.3 8c-2.3 4.1-8.5 8-8.5 8Z" />
    </svg>
  );
}

function UserIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z" />
      <path d="M4 20a8 8 0 0 1 16 0" />
    </svg>
  );
}

function StarIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="m12 3 2.9 5.9 6.5.9-4.7 4.6 1.1 6.5L12 18l-5.8 3 1.1-6.5L2.6 9.8l6.5-.9Z" />
    </svg>
  );
}

function NavItem({ icon, label, active = false }) {
  return (
    <button className={active ? "active" : ""} type="button" aria-label={label}>
      {icon}
      <span>{label}</span>
    </button>
  );
}

export default function App() {
  const [query, setQuery] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("Tudo");
  const [visibleCount, setVisibleCount] = useState(PAGE_SIZE);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [isPending, startTransition] = useTransition();
  const sentinelRef = useRef(null);

  const deferredQuery = useDeferredValue(query);

  const filteredFeed = useMemo(() => {
    const normalizedQuery = deferredQuery.trim().toLowerCase();

    return enrichedFeed.filter((item) => {
      const categoryMatch = selectedCategory === "Tudo" || item.category === selectedCategory;
      const queryMatch =
        normalizedQuery.length === 0 ||
        item.product.toLowerCase().includes(normalizedQuery) ||
        item.store.toLowerCase().includes(normalizedQuery) ||
        item.category.toLowerCase().includes(normalizedQuery);

      return categoryMatch && queryMatch;
    });
  }, [deferredQuery, selectedCategory]);

  useEffect(() => {
    setVisibleCount(PAGE_SIZE);
  }, [deferredQuery, selectedCategory]);

  const visibleProducts = filteredFeed.slice(0, visibleCount);
  const hasMore = visibleCount < filteredFeed.length;

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (!entries[0]?.isIntersecting || !hasMore || isLoadingMore) return;
        setIsLoadingMore(true);
      },
      { rootMargin: "650px 0px" }
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasMore, isLoadingMore]);

  useEffect(() => {
    if (!isLoadingMore) return;

    const timer = setTimeout(() => {
      startTransition(() => {
        setVisibleCount((previous) => Math.min(previous + PAGE_SIZE, filteredFeed.length));
      });
      setIsLoadingMore(false);
    }, 320);

    return () => clearTimeout(timer);
  }, [filteredFeed.length, isLoadingMore, startTransition]);

  return (
    <div className="market-page">
      <main className="market-shell">
        <header className="market-header">
          <div className="header-top">
            <h1 className="market-brand">
              <span>Omni</span>Market
            </h1>
            <div className="header-actions">
              <button type="button" aria-label="Notificacoes">
                <BellIcon />
              </button>
              <button type="button" aria-label="Carrinho">
                <CartIcon />
              </button>
            </div>
          </div>

          <div className="search-bar">
            <SearchIcon />
            <input
              type="search"
              placeholder="Buscar produtos, marcas e lojas"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
            />
          </div>
          <p className="delivery-line">Entrega para Sao Paulo, SP</p>
        </header>

        <section className="hero-row" aria-label="Promocoes principais">
          <article className="hero-card gold">
            <small>OFERTA DO DIA</small>
            <h2>Ate 35% OFF em tecnologia</h2>
            <p>Descontos progressivos com pagamento seguro.</p>
          </article>
          <article className="hero-card dark">
            <small>OMNIPAY</small>
            <h2>Parcele em 10x sem juros</h2>
            <p>Compra protegida para cliente e vendedor.</p>
          </article>
        </section>

        <section className="category-strip" aria-label="Categorias">
          {categories.map((category) => (
            <button
              key={category}
              type="button"
              className={selectedCategory === category ? "active" : ""}
              onClick={() => setSelectedCategory(category)}
            >
              {category}
            </button>
          ))}
        </section>

        <section className="trust-strip" aria-label="Beneficios">
          <p>Compra protegida</p>
          <p>Devolucao facil</p>
          <p>Frete rastreavel</p>
        </section>

        <section className="section-head">
          <div>
            <h3>Ofertas para voce</h3>
            <p>{filteredFeed.length} produtos encontrados</p>
          </div>
          <button type="button">Ver tudo</button>
        </section>

        <section className="product-grid">
          {visibleProducts.map((item) => (
            <article className="product-card" key={item.id}>
              <div className="image-wrap">
                <img src={item.image} alt={item.product} />
                {item.full && <span className="badge full">FULL</span>}
                {item.bestSeller && <span className="badge seller">Mais vendido</span>}
              </div>

              <div className="product-content">
                <p className="store-name">{item.store}</p>
                <h4>{item.product}</h4>

                <p className="main-price">{formatPrice(item.price)}</p>
                <div className="price-row">
                  <span className="old-price">{formatPrice(item.oldPrice)}</span>
                  <span className="discount">{item.discountPercent}% OFF</span>
                </div>
                <p className="installment">10x de {formatPrice(item.price / 10)} sem juros</p>

                <div className="shipping-row">
                  {item.freeShipping ? "Frete gratis" : "Entrega rapida"}
                  {item.lowStock && <span>Ultimas unidades</span>}
                </div>

                <div className="meta-row">
                  <span className="rating">
                    <StarIcon />
                    {item.rating}
                  </span>
                  <span className="reviews">({item.reviews})</span>
                </div>

                <button className="buy-btn" type="button">
                  Adicionar ao carrinho
                </button>
              </div>
            </article>
          ))}
        </section>

        <div ref={sentinelRef} className="sentinel" />

        {(isLoadingMore || isPending) && (
          <p className="status-message">Carregando mais produtos...</p>
        )}
        {!hasMore && <p className="status-message">Fim dos resultados.</p>}
      </main>

      <nav className="bottom-nav" aria-label="Navegacao principal">
        <NavItem active icon={<HomeIcon />} label="Inicio" />
        <NavItem icon={<GridIcon />} label="Categorias" />
        <NavItem icon={<HeartIcon />} label="Favoritos" />
        <NavItem icon={<UserIcon />} label="Perfil" />
      </nav>
    </div>
  );
}
