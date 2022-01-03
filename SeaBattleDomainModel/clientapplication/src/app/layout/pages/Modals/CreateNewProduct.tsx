import { Formik } from 'formik';
import React from 'react'
import { Button, Form, Icon, Modal, TextArea } from 'semantic-ui-react'
import MyTextInput from '../../../common/MyTextInput';
import { useStore } from '../../../stores/store';

interface Props{
    trigger: React.ReactNode
}

function ModalExampleModal(props : Props) {
  const [open, setOpen] = React.useState(false)
  const {productStore} = useStore();
    
function handleNewProductCreation( values : any,
    setErrors: (errors: import("formik")
               .FormikErrors<{ title: string; price: string; description: string; error: string | null; }>) => void): any {


                  console.log(values);
        productStore.createProduct(values);
        setOpen(false);
}

  return (
    <Modal
      onClose={() =>setOpen(false)}
      onOpen={() => setOpen(true)}
      open={open}
      trigger={props.trigger}
    >
      <Modal.Header>Add new product</Modal.Header>
      <Modal.Content>
            <Formik 
                initialValues ={{
                    id: 'id', 
                    title: '',
                    price: 0,
                    description: 'description',
                    error: null
            }}
            onSubmit={(initialValues, {setErrors}) => handleNewProductCreation(initialValues, setErrors)}
        >
            {({handleSubmit, isSubmitting, errors}) => (
            <Form className="ui form" onSubmit={handleSubmit} autoComplete="off" size="large">
                <MyTextInput name = 'title' placeholder='Product title' type='text'/>
                <MyTextInput name = 'price' placeholder='Product price' type='number'/>
                <MyTextInput name = 'description' placeholder='Description' type='text'/>
                
                {renderModalButtons(isSubmitting)}
            </Form>
            )}
        </Formik>
      </Modal.Content>
    </Modal>
  )

  function renderModalButtons(isSubmitting: boolean) {
    return <Modal.Actions style={{ marginTop: '20px' }}>
      <Button color='black' onClick={() => setOpen(false)}>
        Cancel
      </Button>
      <Button
        loading={isSubmitting}
        labelPosition='right'
        icon
        positive
        type='submit'>
        <Icon name='checkmark' />
        Create
      </Button>
    </Modal.Actions>;
  }
}

export default ModalExampleModal