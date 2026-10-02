const server = require('express.js')
const app = server()
const port = process.env.PORT || 8091

app.listen(port,()=> {
console.log(`server is runnning on port ${port}...`)
})