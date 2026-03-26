import { useEffect, useRef, useState, useTransition } from "react";
import { mockFeed } from "./data/mockFeed";

const PAGE_SIZE = 4;

const storyStores = [
  ...new Map(
    mockFeed.map((item) => [item.store, { store: item.store, avatar: item.avatar }])
  ).values()
];

function formatPrice(value) {
  return value.toLocaleString("pt-BR", {
    style: "currency",
    currency: "BRL"
  });
}

function IconButton({ label, children, className = "", onRight = false }) {
  return (
    <button
      type="button"
      aria-label={label}
      className={`${className} ${onRight ? "to-right" : ""}`.trim()}
    >
      {children}
    </button>
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

function ChatIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M5 5h14a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2h-8l-4.5 3v-3H5a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2Z" />
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

function SendIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="m3 12 18-8-6.5 8L21 20 3 12Z" />
    </svg>
  );
}

function BookmarkIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M6 3h12a1 1 0 0 1 1 1v17l-7-4-7 4V4a1 1 0 0 1 1-1Z" />
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

function SearchIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M10.8 4a6.8 6.8 0 1 1 0 13.6A6.8 6.8 0 0 1 10.8 4Zm5.6 12 4 4" />
    </svg>
  );
}

function PlusIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 5v14M5 12h14" />
    </svg>
  );
}

function BagIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M6 8h12l-1 12H7L6 8Z" />
      <path d="M9 8a3 3 0 0 1 6 0" />
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

export default function App() {
  const [visibleCount, setVisibleCount] = useState(PAGE_SIZE);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [isPending, startTransition] = useTransition();
  const sentinelRef = useRef(null);

  const visibleFeed = mockFeed.slice(0, visibleCount);
  const hasMore = visibleCount < mockFeed.length;

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (!entries[0]?.isIntersecting || !hasMore || isLoadingMore) return;
        setIsLoadingMore(true);
      },
      { rootMargin: "900px 0px 900px 0px" }
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasMore, isLoadingMore]);

  useEffect(() => {
    if (!isLoadingMore) return;

    const timer = setTimeout(() => {
      startTransition(() => {
        setVisibleCount((previous) => Math.min(previous + PAGE_SIZE, mockFeed.length));
      });
      setIsLoadingMore(false);
    }, 400);

    return () => clearTimeout(timer);
  }, [isLoadingMore, startTransition]);

  return (
    <div className="page-bg">
      <div className="noise-layer" />
      <main className="feed-shell">
        <header className="topbar">
          <h1 className="brand">
            <span className="brand-gold">Omni</span>Market
          </h1>
          <div className="top-icons">
            <span className="icon-wrap" aria-hidden="true">
              <BellIcon />
            </span>
            <span className="icon-wrap" aria-hidden="true">
              <ChatIcon />
            </span>
          </div>
        </header>

        <section className="stories" aria-label="Lojas em destaque">
          {storyStores.map((story) => (
            <article key={story.store} className="story-item">
              <div className="story-avatar-wrap">
                <img className="story-avatar" src={story.avatar} alt={`Loja ${story.store}`} />
              </div>
              <p className="story-name">{story.store}</p>
            </article>
          ))}
        </section>

        <section className="feed-list">
          {visibleFeed.map((item, index) => (
            <article
              className="product-card"
              key={item.id}
              style={{ animationDelay: `${index * 90}ms` }}
            >
              <div className="card-header">
                <div className="store-meta">
                  <img className="store-avatar" src={item.avatar} alt={item.store} />
                  <strong>{item.store}</strong>
                </div>
                <button className="ghost-btn" type="button" aria-label="Mais opcoes">
                  ...
                </button>
              </div>

              <img className="product-image" src={item.image} alt={item.product} />

              <div className="card-actions">
                <IconButton label="Curtir">
                  <HeartIcon />
                </IconButton>
                <IconButton label="Comentar">
                  <ChatIcon />
                </IconButton>
                <IconButton label="Compartilhar">
                  <SendIcon />
                </IconButton>
                <IconButton label="Salvar" onRight>
                  <BookmarkIcon />
                </IconButton>
              </div>

              <div className="card-content">
                <div className="title-row">
                  <h2>{item.product}</h2>
                  <span className="price">{formatPrice(item.price)}</span>
                </div>
                <p className="description">{item.description}</p>
                <p className="posted">{item.postedAt}</p>
              </div>
            </article>
          ))}

          <div ref={sentinelRef} className="sentinel" />

          {(isLoadingMore || isPending) && (
            <p className="status-message">Carregando mais produtos...</p>
          )}
          {!hasMore && <p className="status-message">Voce chegou ao fim do feed.</p>}
        </section>

        <nav className="bottom-nav" aria-label="Navegacao principal">
          <IconButton className="active" label="Inicio">
            <HomeIcon />
          </IconButton>
          <IconButton label="Buscar">
            <SearchIcon />
          </IconButton>
          <IconButton label="Publicar">
            <PlusIcon />
          </IconButton>
          <IconButton label="Pedidos">
            <BagIcon />
          </IconButton>
          <IconButton label="Perfil">
            <UserIcon />
          </IconButton>
        </nav>
      </main>
    </div>
  );
}
