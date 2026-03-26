# OmniMarket Web (React)

Home principal do TCC em React com:

- Feed estilo marketplace/social
- Rolagem infinita com `IntersectionObserver`
- Paleta baseada na logo (preto + dourado)
- Visual tecnologico responsivo (mobile e desktop)

## Rodar localmente

```bash
cd Omnimarket.Web
npm install
npm run dev
```

Build de producao:

```bash
npm run build
```

## Estrutura principal

- `src/App.jsx`: layout da home + logica de feed infinito
- `src/styles.css`: tema e componentes visuais
- `src/data/mockFeed.js`: dados de exemplo para o feed

## Proximo passo (API real)

Trocar o `mockFeed` por chamada para `GET /api/produto/filtro` da `Omnimarket.Api`, mantendo o mesmo fluxo de pagina:

- `page`
- `pageSize`
- `items`
