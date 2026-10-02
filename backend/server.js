const server = require('express.js')
const app = server()
const cors = require('cors')
const port = process.env.PORT || 8091
app.use(cors())
app.listen(port,()=> {
console.log(`server is runnning on port ${port}...`)
})