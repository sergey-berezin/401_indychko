const PROXY_CONFIG = [
  {
    context: [
      "/api/arcFace",
    ],
    target: "https://localhost:7125",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
