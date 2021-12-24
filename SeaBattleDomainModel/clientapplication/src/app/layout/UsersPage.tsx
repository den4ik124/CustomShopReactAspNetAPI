import { observer } from "mobx-react-lite"
import React, { Fragment } from "react"
import { Header } from "semantic-ui-react"

function UsersPage(){
    return(
        <Fragment>
            <Header> Ussers will be here soon</Header>
        </Fragment>
    )
}

export default observer(UsersPage)
